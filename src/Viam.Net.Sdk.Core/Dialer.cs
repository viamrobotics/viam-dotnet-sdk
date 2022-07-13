namespace Viam.Net.Sdk.Core;

using Grpc.Net.Client;
using Grpc.Core;
using Microsoft.MixedReality.WebRTC;
using Proto.Rpc.V1;
using Proto.Rpc.Webrtc.V1;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

public class Dialer : IDisposable {
    
    private readonly NLog.Logger _logger;

    public Dialer(NLog.Logger logger) {
        _logger = logger;
    }

    static PeerConnectionConfiguration DefaultWebPeerConnectionConfiguration = new PeerConnectionConfiguration {
        IceServers = new List<IceServer> {
            new IceServer { Urls = new List<String> { "stun:global.stun.twilio.com:3478?transport=udp" } }
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

        var config = optsCopy.WebRTCOptions.RTCConfig == null ? DefaultWebPeerConnectionConfiguration : optsCopy.WebRTCOptions.RTCConfig;

        try {
            var configResp = await signalingClient.OptionalWebRTCConfigAsync(new OptionalWebRTCConfigRequest {}, headers: md);
            config = ExtendWebRTCConfig(config, configResp.Config);
        } catch (Grpc.Core.RpcException e) when (e.StatusCode == StatusCode.Unimplemented) {
            // do notihing
        }

        Console.WriteLine("here");

        var (pc, dc) = await NewPeerConnectionForClient(config, optsCopy.WebRTCOptions.DisableTrickleICE);

        Console.WriteLine("here1");

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

            // TODO(erd): if trickle is disabled this will never complete. need to have sdp come from new peer client result
            var sdpReady = new TaskCompletionSource<SdpMessage>();

            if (!optsCopy.WebRTCOptions.DisableTrickleICE) {
                pc.LocalSdpReadytoSend += sdpMessage => sdpReady.SetResult(sdpMessage);
                pc.CreateOffer();

                pc.IceCandidateReadytoSend += i => {
                    // TOOD(erd): check cancelation
                    // if i != nil {
                    //     callFlowWG.Add(1)
                    // }
                    Task.Run(async () => {
                        await remoteDescSet.Task; // TODO(erd): and cancelation
                        if (i == null) {
                            // callFlowWG.Wait()
                            await sendDone();
                            Console.WriteLine("done sending cand");
                            return;
                        }
                        // defer callFlowWG.Done()
                        var iProto = IceCandidateToProto(i);
                        Console.WriteLine("sending cand");
                        await signalingClient.CallUpdateAsync(new CallUpdateRequest { Uuid = uuid, Candidate = iProto }, headers: md);
                    });
                };
            }

            var sdp = await sdpReady.Task;
            var encodedSDP = EncodeSDP(sdp);

            // TODO(erd): cancelation token...
            Console.WriteLine("here2...." + sdp.Content);
            var callClient = signalingClient.Call(new CallRequest{ Sdp = encodedSDP }, headers: md);
            Console.WriteLine("here3");

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

                            await pc.SetRemoteDescriptionAsync(answer);
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
                            pc.AddIceCandidate(cand);
                            break;
                        default:
                            throw new Exception("unexpected stage " + resp.StageCase.ToString());
                    }
                    Console.WriteLine("!!" + resp);
                }
                Console.WriteLine("done...");
                return;
            };

            await Task.Run(exchangeCandidates); // TODO(erd): await first? like a select

            successful = true;
            Console.WriteLine("here4");
            await clientCh.Ready();
            Console.WriteLine("here5");
            return clientCh;
        } finally {
            if (!successful) {
                pc.Dispose();
            }
        }
    }

    private static IceCandidate IceCandidateFromProto(ICECandidate i) {
        return new IceCandidate() {
            // TODO(erd): missing uname frag
            Content = i.Candidate,
            SdpMid = i.SdpMid,
            SdpMlineIndex = (ushort) i.SdpmLineIndex,
        };
    }

    private static ICECandidate IceCandidateInitToProto(IceCandidate ij) {
        return new ICECandidate() {
            Candidate = ij.Content,
            SdpMid = ij.SdpMid,
            SdpmLineIndex = (uint) ij.SdpMlineIndex,
        };
    }

    private static ICECandidate IceCandidateToProto(IceCandidate ij) {
        return new ICECandidate() {
            Candidate = ij.Content,
            SdpMid = ij.SdpMid,
            SdpmLineIndex = (uint) ij.SdpMlineIndex,
        };
    }

    private static string EncodeSDP(SdpMessage localDescription) {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        new JsonObject {
            ["type"] = SdpMessage.TypeToString(localDescription.Type),
            ["sdp"] = localDescription.Content
        }.WriteTo(writer);
        writer.Flush();
        return System.Convert.ToBase64String(stream.ToArray());
    }

    private static SdpMessage DecodeSDP(string encodedSDP) {
        var sdpJSON = System.Convert.FromBase64String(encodedSDP);
        var sdpDoc = JsonDocument.Parse(sdpJSON).RootElement;

        return new SdpMessage {
            Type = SdpMessage.StringToType(sdpDoc.GetProperty("type").GetString()!),
            Content = sdpDoc.GetProperty("sdp").GetString()!
        };
    }

    private static async Task<(PeerConnection, DataChannel)> NewPeerConnectionForClient(PeerConnectionConfiguration config, bool disableTrickleICE) {
        var pc = new PeerConnection();
        await pc.InitializeAsync(config); // TODO(erd): cancelation token

        var successful = false;
        try {
            var dataChannel = await pc.AddDataChannelAsync(0, "data", true, true);
            // TODO(erd): even necessary?
            await pc.AddDataChannelAsync(1, "negotiation", true, true);

            if (disableTrickleICE) {
                // TODO(erd): implement
                throw new NotImplementedException("disabling trickle ICE not supported yet");
            }

            successful = true;
            return (pc, dataChannel);
        } finally {
            if (!successful) {
                pc.Dispose();
            }
        }
    }

    private static PeerConnectionConfiguration ExtendWebRTCConfig(PeerConnectionConfiguration original, WebRTCConfig optional) {
        if (optional == null) {
            return original;
        }

        var extended = new PeerConnectionConfiguration();
        extended.IceServers = new List<IceServer>(original.IceServers);
        extended.IceTransportType = original.IceTransportType;
        extended.BundlePolicy = original.BundlePolicy;
        extended.SdpSemantic = original.SdpSemantic;

        foreach (ICEServer server in optional.AdditionalIceServers) {
            extended.IceServers.Add(new IceServer {
                Urls = server.Urls.ToList(),
                TurnUserName = server.Username,
                TurnPassword = server.Credential
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
