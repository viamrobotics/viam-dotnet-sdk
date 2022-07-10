namespace Viam.Net.Sdk.Core;

using Proto.Rpc.V1;

public record class DialOptions(
    bool Insecure = false,
    string AuthEntity = null!,
    Credentials Credentials = null!,
    DialWebRTCOptions WebRTCOptions = null!,
    string ExternalAuthAddress = null!,
    string ExternalAuthToEntity = null!,
    bool ExternalAuthInsecure = false,
    string Authority = null!
);
