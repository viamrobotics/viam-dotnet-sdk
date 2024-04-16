using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Proto.Rpc.Webrtc.V1;
using SIPSorcery.Net;
using Viam.Net.Sdk.Core.Dialing;
using Viam.Net.Sdk.Core.Grpc;
using Metadata = Grpc.Core.Metadata;

namespace Viam.Net.Sdk.Core.WebRTC
{
    internal class WebRtcDialer(ILogger logger, GrpcDialer grpcDialer)
    {
        internal class DialState(string uuid)
        {
            public string Uuid { get; set; } = uuid;
        }

        public async ValueTask<ViamChannel> DialDirectAsync(WebRtcDialOptions dialOptions)
        {
            logger.LogDebug("Dialing WebRTC to {address} with {signalingServer}", dialOptions.MachineAddress, dialOptions.SignalingAddress);

            var md = new Metadata { { "rpc-host", dialOptions.MachineAddress } };

            var signalingDialOptions = dialOptions.SignalingOptions;

            var signalingChannel = await grpcDialer.DialDirectAsync(signalingDialOptions);
            logger.LogDebug("connected to signaling channel");
            var signalingClient = new SignalingService.SignalingServiceClient(signalingChannel);
            var config = dialOptions.WebRtcOptions.RtcConfig;
            try
            {
                var configResponse =
                    await signalingClient.OptionalWebRTCConfigAsync(new OptionalWebRTCConfigRequest(), headers: md)
                                         .ConfigureAwait(false);

                config = PopulateWithDefaultICEServers(config, configResponse.Config);
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.Unimplemented)
            {
                // Eat it...
            }

            var (peerConnection, dataChannel) = await NewPeerConnectionForClient(config, dialOptions.WebRtcOptions.DisableTrickleIce)
                                                    .ConfigureAwait(false);

            var successful = false;
            var dialState = new DialState(string.Empty);
            try
            {
                var remoteDescriptionSent = new TaskCompletionSource<bool>();
                if (!dialOptions.WebRtcOptions.DisableTrickleIce)
                {
                    var offer = peerConnection.createOffer();

                    peerConnection.onicegatheringstatechange += async state =>
                    {
                        switch (state)
                        {
                            case RTCIceGatheringState.complete:
                                await remoteDescriptionSent.Task.ConfigureAwait(false); // TODO(erd): and cancellation
                                // callFlowWG.Wait()
                                await SendDone(signalingClient, dialState.Uuid, md);
                                break;
                        }
                    };

                    peerConnection.onicecandidate += async i =>
                    {
                        var uuid = await remoteDescriptionSent.Task.ConfigureAwait(false); // TODO(erd): and cancellation
                        var iProto = IceCandidateToProto(i);
                        await signalingClient
                              .CallUpdateAsync(new CallUpdateRequest { Uuid = dialState.Uuid, Candidate = iProto },
                                               headers: md)
                              .ConfigureAwait(false);
                    };

                    await peerConnection.setLocalDescription(offer)
                                        .ConfigureAwait(false);
                }

                var encodedSdp = EncodeSdp(peerConnection.localDescription);

                // TODO(erd): cancellation token...
                var callingClient = signalingClient.Call(new CallRequest { Sdp = encodedSdp }, headers: md);

                // TODO(GOUT-11): do separate auth here
                //if (dialOptions.ExternalAuthAddress != "")
                //{
                //    // TODO(GOUT-11): prepare AuthenticateTo here
                //    // for client channel.
                //}
                //else if (dialOptions.Credentials != null)
                //{
                //    // TODO(GOUT-11): prepare Authenticate here
                //    // for client channel
                //}

                var clientChannel = new WebRTCClientChannel(peerConnection, dataChannel, logger);

                await ExchangeCandidates(signalingClient,
                                         callingClient,
                                         peerConnection,
                                         remoteDescriptionSent,
                                         dialState.Uuid,
                                         md,
                                         dialOptions.WebRtcOptions.DisableTrickleIce, dialState)
                    .ConfigureAwait(false);

                successful = true;
                await clientChannel.Ready()
                                   .ConfigureAwait(false);

                return clientChannel;
            }
            finally
            {
                if (!successful)
                    peerConnection.Close("Failed to dial");
            }
        }

        private static string EncodeSdp(RTCSessionDescription localDescription)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);
            new JsonObject
            {
                ["type"] = localDescription.type.ToString(),
                ["sdp"] = localDescription.sdp.ToString(),
            }.WriteTo(writer);
            writer.Flush();
            return Convert.ToBase64String(stream.ToArray());
        }

        private static RTCSessionDescription DecodeSdp(string encodedSdp)
        {
            var sdpJSON = System.Convert.FromBase64String(encodedSdp);
            var sdpDoc = JsonDocument.Parse(sdpJSON).RootElement;

            return new RTCSessionDescription
            {
                type = Enum.Parse<RTCSdpType>(sdpDoc.GetProperty("type").GetString()!),
                sdp = SDP.ParseSDPDescription(sdpDoc.GetProperty("sdp").GetString()!)
            };
        }

        private async Task ExchangeCandidates(SignalingService.SignalingServiceClient signalingClient,
                                              AsyncServerStreamingCall<CallResponse> callingClient,
                                              RTCPeerConnection peerConnection,
                                              TaskCompletionSource<bool> remoteDescriptionSent,
                                              string expectedUuid,
                                              Metadata md,
                                              bool disableTrickleICE,
                                              DialState state)
        {
            var haveInit = false;
            while(true)
            {
                if (!(await callingClient.ResponseStream.MoveNext()))
                {
                    break;
                }

                var resp = callingClient.ResponseStream.Current;
                switch (resp.StageCase)
                {
                    case CallResponse.StageOneofCase.Init:
                        if (haveInit)
                        {
                            throw new Exception("got init stage more than once");
                        }

                        haveInit = true; 
                        state.Uuid = resp.Uuid;
                        var answer = DecodeSdp(resp.Init.Sdp);

                        peerConnection.setRemoteDescription(
                            new RTCSessionDescriptionInit { type = answer.type, sdp = answer.sdp.ToString() });
                        
                        remoteDescriptionSent.SetResult(true);

                        if (disableTrickleICE)
                        {
                            await SendDone(signalingClient, state.Uuid, md)
                                .ConfigureAwait(false);

                            return;
                        }

                        break;
                    case CallResponse.StageOneofCase.Update:
                        if (!haveInit)
                        {
                            throw new Exception("got update stage before init stage");
                        }

                        if (resp.Uuid != state.Uuid)
                        {
                            throw new Exception($"uuid mismatch; have={resp.Uuid} want={expectedUuid}");
                        }

                        var candidate = IceCandidateFromProto(resp.Update.Candidate);
                        peerConnection.addIceCandidate(candidate);
                        break;
                    default:
                        throw new Exception($"unexpected stage {resp.StageCase}");
                }
            }
        }

        private static ICECandidate IceCandidateToProto(RTCIceCandidate ij)
        {
            var candidate = new ICECandidate() { Candidate = ij.candidate, SdpmLineIndex = ij.sdpMLineIndex, };
            if (ij.sdpMid != null)
            {
                candidate.SdpMid = ij.sdpMid;
            }

            if (ij.usernameFragment != null)
            {
                candidate.UsernameFragment = ij.usernameFragment;
            }

            return candidate;
        }

        private static RTCIceCandidateInit IceCandidateFromProto(ICECandidate i) =>
            new RTCIceCandidateInit()
            {
                candidate = i.Candidate,
                sdpMid = i.SdpMid,
                sdpMLineIndex = (ushort)i.SdpmLineIndex,
                usernameFragment = i.UsernameFragment,
            };

        private static async Task SendDone(SignalingService.SignalingServiceClient signalingClient, string uuid, Metadata md)
        {
            await signalingClient.CallUpdateAsync(new CallUpdateRequest { Uuid = uuid, Done = true }, headers: md)
                                 .ConfigureAwait(false);
        }

        private static async Task<(RTCPeerConnection Connection, RTCDataChannel Channel)> NewPeerConnectionForClient(
            RTCConfiguration config,
            bool disableTrickleICE)
        {
            var pc = new RTCPeerConnection(config);

            var successful = false;
            try
            {
                var dataChannel =
                    await pc.createDataChannel("data",
                                               new RTCDataChannelInit { id = 0, negotiated = true, ordered = true, })
                            .ConfigureAwait(false);

                // TODO(erd): even necessary?
                await pc.createDataChannel("negotiation",
                                           new RTCDataChannelInit { id = 1, negotiated = true, ordered = true, })
                        .ConfigureAwait(false);

                // TODO(erd): remember to remove
                dataChannel.onerror += InitialDataChannelOnError;

                if (disableTrickleICE)
                {
                    throw new NotImplementedException("disabling trickle ICE not supported yet");
                }

                successful = true;
                return (pc, dataChannel);

                void InitialDataChannelOnError(string err)
                {
                    pc.Close($"premature data channel error before WebRTC channel association: {err}");
                }
            }
            finally
            {
                if (!successful)
                {
                    pc.Close("failed to prepare peer connection");
                }
            }
        }

        // TODO: This should probably move to a class where the config lives? Maybe an extension method?
        private static RTCConfiguration PopulateWithDefaultICEServers(RTCConfiguration original, WebRTCConfig? optional)
        {
            if (optional == null)
            {
                return original;
            }

            var extended = new RTCConfiguration
            {
                iceServers = [.. original.iceServers],
                iceTransportPolicy = original.iceTransportPolicy,
                bundlePolicy = original.bundlePolicy,
                rtcpMuxPolicy = original.rtcpMuxPolicy,
                certificates = original.certificates,
                certificates2 = original.certificates2,
                X_DisableExtendedMasterSecretKey = original.X_DisableExtendedMasterSecretKey,
                iceCandidatePoolSize = original.iceCandidatePoolSize,
                X_BindAddress = original.X_BindAddress,
                X_UseRtpFeedbackProfile = original.X_UseRtpFeedbackProfile,
                X_ICEIncludeAllInterfaceAddresses = original.X_ICEIncludeAllInterfaceAddresses
            };

            foreach (var server in optional.AdditionalIceServers)
            {
                extended.iceServers.Add(new RTCIceServer
                {
                    urls = string.Join(",", server.Urls),
                    username = server.Username,
                    credential = server.Credential
                });
            }

            return extended;
        }
    }
}
