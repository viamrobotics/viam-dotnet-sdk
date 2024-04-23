﻿using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Viam.Core.Grpc;
using Viam.Core.WebRTC;

namespace Viam.Core.Dialing
{
    public class Dialer
    {
        private readonly GrpcDialer _grpcDialer;
        private readonly WebRtcDialer _webRtcDialer;

        public Dialer(ILogger logger)
        {
            _grpcDialer = new GrpcDialer(logger);
            _webRtcDialer = new WebRtcDialer(logger, _grpcDialer);
        }

        public ValueTask<ViamChannel> DialGrpcDirectAsync(GrpcDialOptions dialOptions) => _grpcDialer.DialDirectAsync(dialOptions);

        public ValueTask<ViamChannel> DialWebRtcDirectAsync(WebRtcDialOptions dialOptions) =>
            _webRtcDialer.DialDirectAsync(dialOptions);
    }
}
