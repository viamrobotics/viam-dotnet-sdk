namespace Viam.Net.Sdk.Core;

using Grpc.Net.Client;
using Grpc.Core;
using Proto.Rpc.V1;
using Proto.Rpc.Webrtc.V1;
using SIPSorcery.Net;
using System.Runtime.CompilerServices;

public class Dialer : IDisposable {
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

    public async Task<GrpcChannel> DialWebRTCAsync(String signalingAddress, String host, DialOptions opts) {
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
                optsCopy = optsCopy with { AuthEntity = host };
            } else {
                optsCopy = optsCopy with { AuthEntity = optsCopy.ExternalAuthToEntity };
            }
        }

        var signalChannel = await DialDirectGRPCAsync(signalingAddress, optsCopy);
        
        var signalingClient = new SignalingService.SignalingServiceClient(signalChannel);

        // TODO(erd): catch and ignore unimpl
        var configResp = await signalingClient.OptionalWebRTCConfigAsync(new OptionalWebRTCConfigRequest {});

        var config = optsCopy.WebRTCOptions.RTCConfig == null ? DefaultWebRTCConfiguration : optsCopy.WebRTCOptions.RTCConfig;
        var extendedConfig = ExtendWebRTCConfig(config, configResp.Config);
        var (pc, dc) = NewPeerConnectionForClient(extendedConfig, optsCopy.WebRTCOptions.DisableTrickleICE);

        // TODO(erd): locks

        if (optsCopy.WebRTCOptions.DisableTrickleICE) {
            throw new NotImplementedException("disabling trickle ICE not supported yet");
        }

        // TODO(erd): replace with WebRTCChannel
        return signalChannel;
    }

    private static (RTCPeerConnection, RTCDataChannel) NewPeerConnectionForClient(RTCConfiguration config, bool disableTrickleICE) {
        var pc = new RTCPeerConnection(config);

        var dataChannel = pc.createDataChannel("data", new RTCDataChannelInit {
            id = 0,
            negotiated = true,
            ordered = true,
        });

        // TODO(erd): left off here
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
