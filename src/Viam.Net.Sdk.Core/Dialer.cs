namespace Viam.Net.Sdk.Core;

using Grpc.Net.Client;
using Grpc.Core;
using Proto.Rpc.V1;
using Proto.Rpc.Webrtc.V1;
using SIPSorcery.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

public class Dialer : IDisposable {
    
    private readonly NLog.Logger _logger;

    public Dialer(NLog.Logger logger) {
        _logger = logger;
    }

    static RTCConfiguration DefaultWebRTCConfiguration = new RTCConfiguration {
        iceServers = new List<RTCIceServer> {
            new RTCIceServer { urls = "stun:global.stun.twilio.com:3478?transport=udp" }
        }
    };

    public async Task<GrpcChannel> DialDirectGRPCAsync(String address, DialOptions opts) {
        var chanOpts = new GrpcChannelOptions();
        chanOpts.Credentials = opts.Insecure ? ChannelCredentials.Insecure : ChannelCredentials.SecureSsl;

        if (opts.Credentials != null) {
            chanOpts.UnsafeUseInsecureChannelCallCredentials = opts.Insecure;

            string? accessToken = null;
            var accessTokenSema = new SemaphoreSlim(1, 1);

            // TODO(erd): nested async okay?
            var callCreds = CallCredentials.FromInterceptor(async (context, metadata) => {
                using(await accessTokenSema) {
                    if (string.IsNullOrEmpty(accessToken)) {
                        using (var chan = Grpc.Net.Client.GrpcChannel.ForAddress(address)) {
                            // TODO(erd): error handling
                            var authClient = new AuthService.AuthServiceClient(chan);
                            var authResp = await authClient.AuthenticateAsync(new AuthenticateRequest { 
                                Entity = string.IsNullOrEmpty(opts.AuthEntity) ? address : opts.AuthEntity,
                                Credentials = opts.Credentials
                            });
                            accessToken = authResp.AccessToken;
                        }
                    }

                    metadata.Add("Authorization", $"Bearer {accessToken}");   
                }
            });
            chanOpts.Credentials = opts.Insecure ? new UnsafeInsecureChannelCredentials(callCreds) : ChannelCredentials.Create(chanOpts.Credentials, callCreds);
        }

        var chan = Grpc.Net.Client.GrpcChannel.ForAddress(address, chanOpts);
        await chan.ConnectAsync();

        return new GrpcChannel.Wrapped(chan);
    }

    // TODO(erd): timeout+cancelation
    public async Task<GrpcChannel> DialWebRTCAsync(String signalingAddress, String host, DialOptions opts) {
        _logger.Debug("connecting to signaling server {} and host {}", signalingAddress, host);
        var md = new Grpc.Core.Metadata();
        md.Add("rpc-host", host);

        var optsCopy = opts with {};

        optsCopy = optsCopy with { Insecure = opts.WebRTCOptions != null && opts.WebRTCOptions.SignalingInsecure };
        if (optsCopy.WebRTCOptions == null) {
            optsCopy = optsCopy with { WebRTCOptions = new DialWebRTCOptions {} };
        }

        optsCopy = optsCopy with {
            WebRTCOptions = optsCopy.WebRTCOptions with { SignalingServerAddress = signalingAddress }
        };

        if (opts.WebRTCOptions != null) {
            optsCopy = optsCopy with {
                Credentials = opts.WebRTCOptions.SignalingCredentials,
                ExternalAuthAddress = opts.WebRTCOptions.SignalingExternalAuthAddress,
                ExternalAuthToEntity = opts.WebRTCOptions.SignalingExternalAuthToEntity,
                ExternalAuthInsecure = opts.WebRTCOptions.SignalingExternalAuthInsecure
            };
        }

        if (string.IsNullOrEmpty(optsCopy.AuthEntity)) {
            if (string.IsNullOrEmpty(optsCopy.ExternalAuthAddress)) {
                _logger.Debug("auth entity empty; setting to host {}", host);
                optsCopy = optsCopy with { AuthEntity = host };
            } else {
                _logger.Debug("auth entity empty; setting to external auth address {}", optsCopy.ExternalAuthAddress);
                optsCopy = optsCopy with { AuthEntity = optsCopy.ExternalAuthToEntity };
            }
        }

        var signalChannel = await DialDirectGRPCAsync(signalingAddress, optsCopy);

        _logger.Debug("connected");
        
        var signalingClient = new SignalingService.SignalingServiceClient(signalChannel);

        var config = optsCopy.WebRTCOptions.RTCConfig == null ? DefaultWebRTCConfiguration : optsCopy.WebRTCOptions.RTCConfig;

        try {
            var configResp = await signalingClient.OptionalWebRTCConfigAsync(new OptionalWebRTCConfigRequest {}, headers: md);
            config = ExtendWebRTCConfig(config, configResp.Config);
        } catch (Grpc.Core.RpcException e) when (e.StatusCode == StatusCode.Unimplemented) {
            // do notihing
        }

        var (pc, dc) = await NewPeerConnectionForClient(config, optsCopy.WebRTCOptions.DisableTrickleICE);

        var successful = false;

        var uuid = "";

        // only send once since exchange may end or ICE may end
        var sendDone = async () => {
            // TODO(erd): make sure only sending once
            await signalingClient.CallUpdateAsync(new CallUpdateRequest { Uuid = uuid, Done = true }, headers: md);
        };

        try {
            // TODO(erd): locks
            // TODO(erd): cancelation token...
            // TODO(erd): sendErr on failures...

            var remoteDescSet = new TaskCompletionSource<bool>();
            if (!optsCopy.WebRTCOptions.DisableTrickleICE) {
                var offer = pc.createOffer();

                pc.onicegatheringstatechange += async state => {
                    switch (state) {
                        case RTCIceGatheringState.complete:
                            await remoteDescSet.Task; // TODO(erd): and cancelation
                            // callFlowWG.Wait()
                            await sendDone();
                            break;
                    }
                };
                pc.onicecandidate += i => {
                    // TOOD(erd): check cancelation
                    // if i != nil {
                    //     callFlowWG.Add(1)
                    // }
                    Task.Run(async () => {
                        await remoteDescSet.Task; // TODO(erd): and cancelation
                        // defer callFlowWG.Done()
                        var iProto = IceCandidateToProto(i);
                        await signalingClient.CallUpdateAsync(new CallUpdateRequest { Uuid = uuid, Candidate = iProto }, headers: md);
                    });
                };

                await pc.setLocalDescription(offer);
            }

            var encodedSDP = EncodeSDP(pc.localDescription);

            // TODO(erd): cancelation token...
            var callClient = signalingClient.Call(new CallRequest{ Sdp = encodedSDP }, headers: md);

            // TODO(GOUT-11): do separate auth here
            if (opts.ExternalAuthAddress != "") {
                // TODO(GOUT-11): prepare AuthenticateTo here
                // for client channel.
            } else if (opts.Credentials != null) {
                // TODO(GOUT-11): prepare Authenticate here
                // for client channel
            }

            var clientCh = new WebRTCClientChannel(pc, dc, _logger);

            // TODO(erd): task.run this
            var exchangeCandidates = async () => {
                var haveInit = false;
                await foreach (var resp in callClient.ResponseStream.ReadAllAsync()) {
                    // TODO(erd): check cancelled

                    switch (resp.StageCase) {
                        case CallResponse.StageOneofCase.Init:
                            if (haveInit) {
                                throw new Exception("got init stage more than once");
                            }
                            haveInit = true;
                            uuid = resp.Uuid;
                            var answer = DecodeSDP(resp.Init.Sdp);

                            pc.setRemoteDescription(new RTCSessionDescriptionInit { type = answer.type, sdp = answer.sdp.ToString() });
                            remoteDescSet.SetResult(true);

                            if (optsCopy.WebRTCOptions.DisableTrickleICE) {
                                await sendDone();
                                return;
                            }
                            break;
                        case CallResponse.StageOneofCase.Update:
                            if (!haveInit) {
                                throw new Exception("got update stage before init stage");
                            }
                            if (resp.Uuid != uuid) {
                                throw new Exception(String.Format("uuid mismatch; have=%s want=%s", resp.Uuid, uuid));
                            }
                            var cand = IceCandidateFromProto(resp.Update.Candidate);
                            pc.addIceCandidate(cand);
                            break;
                        default:
                            throw new Exception("unexpected stage " + resp.StageCase.ToString());
                    }
                }
                return;
            };

            await Task.Run(exchangeCandidates); // TODO(erd): await first? like a select

            successful = true;
            await clientCh.Ready();
            return clientCh;
        } finally {
            if (!successful) {
                pc.Close("failed to dial");
            }
        }
    }

    private static RTCIceCandidateInit IceCandidateFromProto(ICECandidate i) {
        return new RTCIceCandidateInit() {
            candidate = i.Candidate,
            sdpMid = i.SdpMid,
            sdpMLineIndex = (ushort) i.SdpmLineIndex,
            usernameFragment = i.UsernameFragment,
        };
    }

    private static ICECandidate IceCandidateInitToProto(RTCIceCandidateInit ij) {
        return new ICECandidate() {
            Candidate = ij.candidate,
            SdpMid = ij.sdpMid,
            SdpmLineIndex = ij.sdpMLineIndex,
            UsernameFragment = ij.usernameFragment,
        };
    }

    private static ICECandidate IceCandidateToProto(RTCIceCandidate ij) {
        return new ICECandidate() {
            Candidate = ij.candidate,
            SdpMid = ij.sdpMid,
            SdpmLineIndex = ij.sdpMLineIndex,
            UsernameFragment = ij.usernameFragment,
        };
    }

    private static string EncodeSDP(RTCSessionDescription localDescription) {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        new JsonObject {
            ["type"] = localDescription.type.ToString(),
            ["sdp"] = localDescription.sdp.ToString(),
        }.WriteTo(writer);
        writer.Flush();
        return System.Convert.ToBase64String(stream.ToArray());
    }

    private static RTCSessionDescription DecodeSDP(string encodedSDP) {
        var sdpJSON = System.Convert.FromBase64String(encodedSDP);
        var sdpDoc = JsonDocument.Parse(sdpJSON).RootElement;

        return new RTCSessionDescription {
            type = Enum.Parse<RTCSdpType>(sdpDoc.GetProperty("type").GetString()!),
            sdp = SDP.ParseSDPDescription(sdpDoc.GetProperty("sdp").GetString()!)
        };
    }

    private static async Task<(RTCPeerConnection, RTCDataChannel)> NewPeerConnectionForClient(RTCConfiguration config, bool disableTrickleICE) {
        var pc = new RTCPeerConnection(config);

        var successful = false;
        try {
            var dataChannel = await pc.createDataChannel("data", new RTCDataChannelInit {
                id = 0,
                negotiated = true,
                ordered = true,
            });
            // TODO(erd): even necessary?
            await pc.createDataChannel("negotiation", new RTCDataChannelInit {
                id = 1,
                negotiated = true,
                ordered = true,
            });
            var initialDataChannelOnError = (string err) => {
                pc.Close($"premature data channel error before WebRTC channel association: {err}");
            };
            // TODO(erd): remember to remove
            dataChannel.onerror += initialDataChannelOnError;

            if (disableTrickleICE) {
                // TODO(erd): implement
                throw new NotImplementedException("disabling trickle ICE not supported yet");
            }

            successful = true;
            return (pc, dataChannel);
        } finally {
            if (!successful) {
                pc.Close("failed to prepare peer connection");
            }
        }
    }

    private static RTCConfiguration ExtendWebRTCConfig(RTCConfiguration original, WebRTCConfig optional) {
        if (optional == null) {
            return original;
        }

        var extended = new RTCConfiguration();
        extended.iceServers = new List<RTCIceServer>(original.iceServers);
        extended.iceTransportPolicy = original.iceTransportPolicy;
        extended.bundlePolicy = original.bundlePolicy;
        extended.rtcpMuxPolicy = original.rtcpMuxPolicy;
        extended.certificates = original.certificates;
        extended.certificates2 = original.certificates2;
        extended.X_DisableExtendedMasterSecretKey = original.X_DisableExtendedMasterSecretKey;
        extended.iceCandidatePoolSize = original.iceCandidatePoolSize;
        extended.X_BindAddress = original.X_BindAddress;
        extended.X_UseRtpFeedbackProfile = original.X_UseRtpFeedbackProfile;
        extended.X_ICEIncludeAllInterfaceAddresses = original.X_ICEIncludeAllInterfaceAddresses;

        foreach (ICEServer server in optional.AdditionalIceServers) {
            extended.iceServers.Add(new RTCIceServer {
                urls = string.Join(",", server.Urls),
                username = server.Username,
                credential = server.Credential
            });
        }

        return extended;
    }

    public void Dispose() {
        // TODO(erd): do something?
    }
}

// from https://github.com/dotnet/csharplang/issues/983#issuecomment-529297409
static class SemaphoreSlimExtensions
{
    public static TaskAwaiter<DisposableReleaser> GetAwaiter(this SemaphoreSlim semaphoreSlim) =>
        semaphoreSlim.LockAsync().GetAwaiter();

    public static async Task<DisposableReleaser> LockAsync(this SemaphoreSlim semaphoreSlim, CancellationToken cancellationToken = default)
    {
        await semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new DisposableReleaser(semaphoreSlim);
    }

    public readonly struct DisposableReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public DisposableReleaser(SemaphoreSlim semaphore) =>
            _semaphore = semaphore;

        public void Dispose() =>
            _semaphore.Release();
    }
}
