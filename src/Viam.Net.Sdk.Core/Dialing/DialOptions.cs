using System;
using Viam.Net.Sdk.Core.Options;

namespace Viam.Net.Sdk.Core.Dialing
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
