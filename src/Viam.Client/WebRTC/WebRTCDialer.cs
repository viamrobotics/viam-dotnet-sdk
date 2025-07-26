using Fody;

using Grpc.Core;
using Grpc.Net.Client;

using Microsoft.Extensions.Logging;

using System.Net.Sockets;

using Viam.Core;
using Viam.Core.Grpc;
using Viam.Core.Logging;

namespace Viam.Client.WebRTC
{
    public record WebRtcDialOptions(
        Uri SignalingAddress,
        string MachineAddress,
        GrpcDialOptions SignalingOptions,
        bool InsecureSignaling,
        bool AllowInsecureDowngrade,
        Credentials? Credentials,
        TimeSpan Timeout,
        HttpKeepAlivePingPolicy? KeepAlivePingPolicy,
        TimeSpan? KeepAlivePingDelay,
        TimeSpan? KeepAlivePingTimeout);

    /// <summary>
    /// A Dialer that uses WebRTC to connect to the Smart Machine
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{WebRtcDialer}"/> to use for state logging</param>
    [ConfigureAwait(false)]
    internal class WebRtcDialer(ILogger<WebRtcDialer> logger, ILoggerFactory loggerFactory)
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
        public ValueTask<ViamChannel> DialDirectAsync(WebRtcDialOptions dialOptions,
            CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Dialing WebRTC to {address} with signaling server {signalingServer}",
                dialOptions.MachineAddress, dialOptions.SignalingAddress);
            var allowInsecure = dialOptions.InsecureSignaling
                                || dialOptions.SignalingOptions.Insecure
                                || (dialOptions.Credentials?.Type == null
                                    || dialOptions.Credentials?.Payload == null
                                    && dialOptions.AllowInsecureDowngrade);
            logger.LogTrace("Initializing Rust runtime");
            var runtimePointer = ViamRustUtils.InitRustRuntime();
            try
            {
                logger.LogTrace("Rust runtime initialized");
                var proxyPath = ViamRustUtils.Dial(dialOptions.MachineAddress, dialOptions.Credentials?.AuthEntity,
                    dialOptions.Credentials?.Type, dialOptions.Credentials?.Payload, allowInsecure, (float)dialOptions.Timeout.TotalSeconds,
                    runtimePointer);
                logger.LogTrace("Dialed successfully, got proxy pointer");
                try
                {
                    logger.LogTrace("Parsed proxy address: {path}", proxyPath);
                    var channelOptions = new GrpcChannelOptions()
                    {
                        LoggerFactory = loggerFactory,
                    };
                    Uri uri;
                    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                    {
                        var endpoint = new UnixDomainSocketEndPoint(proxyPath);
                        var factory = new UnixDomainSocketsConnectionFactory(endpoint, logger);
                        var handler = new SocketsHttpHandler()
                        {
                            ConnectCallback = factory.ConnectAsync,
                            UseProxy = false,
                            AllowAutoRedirect = false
                        };

                        if (dialOptions.KeepAlivePingPolicy.HasValue)
                        {
                            handler.KeepAlivePingPolicy = dialOptions.KeepAlivePingPolicy.Value;
                        }

                        if (dialOptions.KeepAlivePingDelay.HasValue)
                        {
                            handler.KeepAlivePingDelay = dialOptions.KeepAlivePingDelay.Value;
                        }

                        if (dialOptions.KeepAlivePingTimeout.HasValue)
                        {
                            handler.KeepAlivePingTimeout = dialOptions.KeepAlivePingTimeout.Value;
                        }

                        channelOptions.HttpHandler = handler;
                        uri = new Uri($"http://localhost:9090");
                    }
                    else if (OperatingSystem.IsWindows())
                    {
                        uri = new Uri($"http://{proxyPath}");
                    }
                    else
                    {
                        throw new Exception("Unknown OS, not sure how to support it.");
                    }

                    logger.LogDebug("Using proxy URI: {uri}", uri);
                    var channel = new WebRTCViamChannel(logger, runtimePointer,
                        Grpc.Net.Client.GrpcChannel.ForAddress(uri, channelOptions),
                        dialOptions.MachineAddress);
                    logger.LogDialComplete();
                    return new ValueTask<ViamChannel>(channel);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to parse proxy address");
                    throw;
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to dial WebRTC");
                logger.LogTrace("Freeing rust runtime");
                ViamRustUtils.FreeRustRuntime(runtimePointer);
                logger.LogDebug("Rust runtime freed");
                throw new AggregateException(new Exception("Failed to dial WebRTC"), ex);
            }
        }
    }

    internal class WebRTCViamChannel(ILogger<WebRtcDialer> logger, IntPtr rustRuntime, global::Grpc.Net.Client.GrpcChannel channel, string remote)
        : ViamChannel(remote)
    {
        public override void Dispose()
        {
            logger.LogDebug("Disposing of channel");
            channel.Dispose();
            logger.LogDebug("Disposed of channel, freeing Rust runtime");
            ViamRustUtils.FreeRustRuntime(rustRuntime);
            logger.LogDebug("Rust runtime freed");
            GC.SuppressFinalize(this);
        }

        protected override CallInvoker GetCallInvoker() => channel.CreateCallInvoker();
    }
}