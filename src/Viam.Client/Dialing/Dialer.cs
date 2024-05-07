using Microsoft.Extensions.Logging;

using Viam.Core.Grpc;
using Viam.Client.WebRTC;
using Viam.Core;

namespace Viam.Client.Dialing
{
    public class Dialer
    {
        private readonly GrpcDialer _grpcDialer;
        private readonly WebRtcDialer _webRtcDialer;

        public Dialer(ILoggerFactory loggerFactory)
        {
            _grpcDialer = new GrpcDialer(loggerFactory.CreateLogger<GrpcDialer>(), loggerFactory);
            _webRtcDialer = new WebRtcDialer(loggerFactory.CreateLogger<WebRtcDialer>(), _grpcDialer);
        }

        public ValueTask<ViamChannel> DialGrpcDirectAsync(GrpcDialOptions dialOptions) => _grpcDialer.DialDirectAsync(dialOptions);

        public ValueTask<ViamChannel> DialWebRtcDirectAsync(WebRtcDialOptions dialOptions) =>
            _webRtcDialer.DialDirectAsync(dialOptions);
    }
}
