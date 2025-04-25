using System.Text.Json;
using System.Text.Json.Nodes;
using Fody;
using Grpc.Core;

using Microsoft.Extensions.Logging;

using Proto.Rpc.Webrtc.V1;

using SIPSorcery.Net;

using Viam.Client.Dialing;
using Viam.Core.Grpc;
using Viam.Core;
using Viam.Core.Logging;
using Metadata = Grpc.Core.Metadata;
using TinyJson;

namespace Viam.Client.WebRTC
{
    public record WebRtcDialOptions(
        Uri SignalingAddress,
        string MachineAddress,
        GrpcDialOptions SignalingOptions,
        WebRtcOptions WebRtcOptions,
        bool InsecureSignaling = false,
        Credentials? Credentials = null);

    /// <summary>
    /// A Dialer that uses WebRTC to connect to the Smart Machine
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{WebRtcDialer}"/> to use for state logging</param>
    /// <param name="grpcDialer">The <see cref="GrpcDialer"/> to use for signaling</param>
    [ConfigureAwait(false)]
    internal class WebRtcDialer(ILogger<WebRtcDialer> logger, GrpcDialer grpcDialer)
    {
        internal class DialState(string uuid)
        {
            public string Uuid { get; set; } = uuid;
        }

        /// <summary>
        /// Dial a Viam Smart Machine using WebRTC
        /// </summary>
        /// <param name="dialOptions">The <see cref="WebRtcDialOptions"/> to use when dialing the smart machine</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the operation</param>
        /// <returns>A <see cref="ValueTask{ViamChannel}"/></returns>
        public async ValueTask<ViamChannel> DialDirectAsync(WebRtcDialOptions dialOptions, CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Dialing WebRTC to {address} with signaling server {signalingServer}", dialOptions.MachineAddress, dialOptions.SignalingAddress);

            var md = new Metadata { { "rpc-host", dialOptions.MachineAddress } };

            var signalingDialOptions = dialOptions.SignalingOptions;

            using var signalingChannel = await grpcDialer.DialDirectAsync(signalingDialOptions, cancellationToken);
            logger.LogDebug("connected to signaling channel");
            var signalingClient = new SignalingService.SignalingServiceClient(signalingChannel);
            var config = dialOptions.WebRtcOptions.RtcConfig;
            try
            {
                var configResponse =
                    await signalingClient.OptionalWebRTCConfigAsync(new OptionalWebRTCConfigRequest(), headers: md);

                config = PopulateWithDefaultIceServers(config, configResponse.Config);
            }
            catch (RpcException e) when (e.StatusCode == StatusCode.Unimplemented)
            {
                // Eat it...
            }

            var (peerConnection, dataChannel) = await NewPeerConnectionForClient(config, dialOptions.WebRtcOptions.DisableTrickleIce);

            var successful = false;
            var dialState = new DialState(string.Empty);
            WebRtcClientChannel? clientChannel = null;
            try
            {
                var remoteDescriptionSent = new TaskCompletionSource<bool>();
                if (!dialOptions.WebRtcOptions.DisableTrickleIce)
                {
                    var offer = peerConnection.createOffer();

                    peerConnection.onicegatheringstatechange += async state =>
                    {
                        logger.LogIceGatheringStateChange(state.ToString());
                        switch (state)
                        {
                            case RTCIceGatheringState.complete:
                                await remoteDescriptionSent.Task; // TODO(erd): and cancellation
                                // callFlowWG.Wait()
                                logger.LogTrace("ICE Gathering Complete");
                                await SendDone(signalingClient, dialState.Uuid, md, cancellationToken);
                                break;
                        }
                    };

                    peerConnection.onicecandidate += async i =>
                    {
                        var uuid = await remoteDescriptionSent.Task; // TODO(erd): and cancellation
                        var iProto = IceCandidateToProto(i);
                        logger.LogTrace("got ice candidate {Candidate}", iProto.Candidate);
                        logger.LogTrace("sending call update for candidate {Candidate}", iProto.Candidate);
                        await signalingClient
                              .CallUpdateAsync(new CallUpdateRequest { Uuid = dialState.Uuid, Candidate = iProto },
                                               headers: md,
                                               deadline: DateTime.UtcNow.AddSeconds(5),
                                               cancellationToken: cancellationToken)
                              .ConfigureAwait(false);

                        logger.LogTrace("ice candidate call update complete {Candidate}", iProto.Candidate);
                    };

                    await peerConnection.setLocalDescription(offer);
                }

                var encodedSdp = EncodeSdp(peerConnection.localDescription);

                // TODO(erd): cancellation token...
                using var callingClient = signalingClient.Call(new CallRequest { Sdp = encodedSdp },
                                                         headers: md,
                                                         cancellationToken: cancellationToken);
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

                clientChannel = new WebRtcClientChannel(peerConnection, dataChannel, logger);

                await ExchangeCandidates(signalingClient,
                                         callingClient,
                                         peerConnection,
                                         remoteDescriptionSent,
                                         dialState.Uuid,
                                         md,
                                         dialOptions.WebRtcOptions.DisableTrickleIce,
                                         dialState,
                                         cancellationToken);

                logger.LogTrace("Waiting for clientChannel ready");
                await clientChannel.Ready();
                successful = true;
                logger.LogTrace("clientChannel ready!");

                return clientChannel;
            }
            finally
            {
                if (!successful)
                {
                    clientChannel?.Dispose();
                }
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
            var sdpJson = Convert.FromBase64String(encodedSdp);
            var sdpDoc = JsonDocument.Parse(sdpJson).RootElement;

            if (Enum.TryParse<RTCSdpType>(sdpDoc.GetProperty("type").GetString()!, out var type))
            {
                return new RTCSessionDescription
                {
                    type = type,
                    sdp = SDP.ParseSDPDescription(sdpDoc.GetProperty("sdp").GetString()!)
                };
            }
            else
            {
                throw new Exception("failed to parse sdp type");
            }
        }

        private async Task ExchangeCandidates(SignalingService.SignalingServiceClient signalingClient,
                                              AsyncServerStreamingCall<CallResponse> callingClient,
                                              IRTCPeerConnection peerConnection,
                                              TaskCompletionSource<bool> remoteDescriptionSent,
                                              string expectedUuid,
                                              Metadata md,
                                              bool disableTrickleIce,
                                              DialState state,
                                              CancellationToken cancellationToken)
        {
            var haveInit = false;
            while (true)
            {
                if (!(await callingClient.ResponseStream.MoveNext()))
                {
                    logger.LogTrace("signaling stream ended");
                    break;
                }

                var resp = callingClient.ResponseStream.Current;
                logger.LogTrace("signaling stream next, stage {Stage}", resp.StageCase);
                switch (resp.StageCase)
                {
                    case CallResponse.StageOneofCase.Init:
                        logger.LogTrace("Candidate Exchange State: Init");
                        if (haveInit)
                        {
                            throw new Exception("got init stage more than once");
                        }

                        haveInit = true;
                        state.Uuid = resp.Uuid;
                        var answer = DecodeSdp(resp.Init.Sdp);

                        logger.LogTrace("Candidate Exchange Response: {Response}", resp.ToJson());
                        peerConnection.setRemoteDescription(new RTCSessionDescriptionInit { type = answer.type, sdp = answer.sdp.ToString() });

                        remoteDescriptionSent.SetResult(true);

                        if (disableTrickleIce)
                        {
                            logger.LogTrace("TrickleICE is disabled");
                            await SendDone(signalingClient, state.Uuid, md, cancellationToken);

                            return;
                        }

                        break;
                    case CallResponse.StageOneofCase.Update:
                        logger.LogTrace("Candidate Exchange State: Update");
                        if (!haveInit)
                        {
                            throw new Exception("got update stage before init stage");
                        }

                        if (resp.Uuid != state.Uuid)
                        {
                            throw new Exception($"uuid mismatch; have={resp.Uuid} want={expectedUuid}");
                        }

                        var candidate = IceCandidateFromProto(resp.Update.Candidate);
                        logger.LogTrace("Got canddidate: {Candidate}", candidate.candidate);
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
            new()
            {
                candidate = i.Candidate,
                sdpMid = i.SdpMid,
                sdpMLineIndex = (ushort)i.SdpmLineIndex,
                usernameFragment = i.UsernameFragment,
            };

        private async Task SendDone(SignalingService.SignalingServiceClient signalingClient, string uuid, Metadata md, CancellationToken cancellationToken)
        {
            logger.LogTrace("sending done to signaling service");
            await signalingClient.CallUpdateAsync(new CallUpdateRequest { Uuid = uuid, Done = true }, headers: md, cancellationToken: cancellationToken);
        }

        private static async Task<(RTCPeerConnection Connection, RTCDataChannel Channel)> NewPeerConnectionForClient(
            RTCConfiguration config,
            bool disableTrickleIce)
        {
            var pc = new RTCPeerConnection(config);

            var successful = false;
            try
            {
                var dataChannel =
                    await pc.createDataChannel("data",
                                               new RTCDataChannelInit { id = 0, negotiated = true, ordered = true, });

                // TODO(erd): even necessary?
                await pc.createDataChannel("negotiation",
                                           new RTCDataChannelInit { id = 1, negotiated = true, ordered = true, });

                // TODO(erd): remember to remove
                dataChannel.onerror += InitialDataChannelOnError;

                if (disableTrickleIce)
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
        private static RTCConfiguration PopulateWithDefaultIceServers(RTCConfiguration original, WebRTCConfig? optional)
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
