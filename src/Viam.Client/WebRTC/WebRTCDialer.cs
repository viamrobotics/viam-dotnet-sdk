using Fody;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Viam.Core;
using Viam.Core.Grpc;
using Viam.Core.Logging;

namespace Viam.Client.WebRTC
{
    public record WebRtcDialOptions(
        Uri signalingAddress,
        string machineAddress,
        GrpcDialOptions signalingOptions,
        bool insecureSignaling = false,
        bool allowInsecureDowngrade = false,
        Credentials? credentials = null,
        float timeout = 30);

    /// <summary>
    /// A Dialer that uses WebRTC to connect to the Smart Machine
    /// </summary>
    /// <param name="logger">The <see cref="ILogger{WebRtcDialer}"/> to use for state logging</param>
    /// <param name="grpcDialer">The <see cref="GrpcDialer"/> to use for signaling</param>
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
                dialOptions.machineAddress, dialOptions.signalingAddress);
            var allowInsecure = dialOptions.insecureSignaling || dialOptions.signalingOptions.Insecure ||
                                (dialOptions.credentials?.Type == null || dialOptions.credentials?.Payload == null &&
                                    dialOptions.allowInsecureDowngrade);
            logger.LogTrace("Initializing Rust runtime");
            var runtimePointer = ViamRustUtils.InitRustRuntime();
            try
            {
                logger.LogTrace("Rust runtime initialized");
                var proxyPath = ViamRustUtils.Dial(dialOptions.machineAddress, dialOptions.credentials?.AuthEntity,
                    dialOptions.credentials?.Type, dialOptions.credentials?.Payload, allowInsecure, dialOptions.timeout,
                    runtimePointer);
                logger.LogTrace("Dialed successfully, got proxy pointer");
                try
                {
                    logger.LogTrace("Parsed proxy address: {path}", proxyPath);
                    var uri = new Uri($"http://{proxyPath}");
                    if (proxyPath.StartsWith("tcp://"))
                    {
                        var proxyAddress = proxyPath.Replace("tcp://", "");
                        uri = new Uri($"http://{proxyAddress}");
                    }

                    if (proxyPath.StartsWith("unix://"))
                    {
                        //var proxyAddress = path.Replace("unix://", "");
                        //uri = new Uri($"http://{proxyAddress}");
                        throw new Exception("Unix domain sockets are not supported yet");
                    }

                    var channelOptions = new GrpcChannelOptions()
                    {
                        LoggerFactory = loggerFactory
                    };
                    var channel = new WebRTCViamChannel(runtimePointer,
                        global::Grpc.Net.Client.GrpcChannel.ForAddress(uri, channelOptions),
                        dialOptions.machineAddress);
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
                ViamRustUtils.FreeRustRuntime(runtimePointer);
            }

            throw new Exception("Failed to dial WebRTC");
        }
    }

    public class WebRTCViamChannel(IntPtr rustRuntime, global::Grpc.Net.Client.GrpcChannel channel, string remote)
        : ViamChannel(remote)
    {
        public override void Dispose()
        {
            channel.Dispose();
            ViamRustUtils.FreeRustRuntime(rustRuntime);
        }

        protected override CallInvoker GetCallInvoker() => channel.CreateCallInvoker();
    }
}