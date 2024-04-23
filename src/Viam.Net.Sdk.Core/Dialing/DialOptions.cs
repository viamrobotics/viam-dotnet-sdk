using System;
using Viam.Core.Options;

namespace Viam.Core.Dialing
{
    public record GrpcDialOptions(Uri MachineAddress, bool Insecure = false, Credentials? Credentials = null, int port = 8080);

    public record WebRtcDialOptions(
        Uri SignalingAddress,
        string MachineAddress,
        GrpcDialOptions SignalingOptions,
        WebRtcOptions WebRtcOptions,
        bool InsecureSignaling = false,
        Credentials? Credentials = null);
}
