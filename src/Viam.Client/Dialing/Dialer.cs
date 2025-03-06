using Microsoft.Extensions.Logging;

using Viam.Core.Grpc;
using Viam.Client.WebRTC;
using Viam.Core;

namespace Viam.Client.Dialing
{
    /// <summary>
    /// A dialer for connecting to a Viam server
    ///
    /// Note: You probably don't need to use this class directly. Instead, use the `RobotClient` and `ViamClient` classes instead. This is exposed for advanced use cases.
    /// </summary>
    public class Dialer
    {
        private readonly GrpcDialer _grpcDialer;
        private readonly WebRtcDialer _webRtcDialer;

        public Dialer(ILoggerFactory loggerFactory)
        {
            if (Environment.GetEnvironmentVariable("ENABLE_SIPSORCERY_LOGGING") != null)
                SIPSorcery.LogFactory.Set(loggerFactory);
            _grpcDialer = new GrpcDialer(loggerFactory.CreateLogger<GrpcDialer>(), loggerFactory);
            _webRtcDialer = new WebRtcDialer(loggerFactory.CreateLogger<WebRtcDialer>(), _grpcDialer);
        }

        public ValueTask<ViamChannel> DialGrpcDirectAsync(GrpcDialOptions dialOptions, CancellationToken cancellationToken = default) =>
            _grpcDialer.DialDirectAsync(dialOptions, cancellationToken);

        public ValueTask<ViamChannel> DialWebRtcDirectAsync(WebRtcDialOptions dialOptions, CancellationToken cancellationToken = default) =>
            _webRtcDialer.DialDirectAsync(dialOptions, cancellationToken);
    }
}
