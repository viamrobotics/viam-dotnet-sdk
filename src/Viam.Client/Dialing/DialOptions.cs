using System;

using Viam.Client.Options;
using Viam.Core.Grpc;
using Viam.Core.Options;

namespace Viam.Client.Dialing
{
    public record WebRtcDialOptions(
        Uri SignalingAddress,
        string MachineAddress,
        GrpcDialOptions SignalingOptions,
        WebRtcOptions WebRtcOptions,
        bool InsecureSignaling = false,
        Credentials? Credentials = null);
}
