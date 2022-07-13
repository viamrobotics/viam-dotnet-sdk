namespace Viam.Net.Sdk.Core;

using Proto.Rpc.V1;
using Microsoft.MixedReality.WebRTC;

public record class DialWebRTCOptions(
    bool SignalingInsecure = false,
    bool DisableTrickleICE = false,

    PeerConnectionConfiguration RTCConfig = null!,

    // SignalingAuthEntity is the entity to authenticate as to the signaler.
    string SignalingAuthEntity = null!,

    // SignalingExternalAuthAddress is the address to perform external auth yet.
    // This is unlikely to be needed since the signaler is typically in the same
    // place where authentication happens.
    string SignalingExternalAuthAddress = null!,

    // SignalingExternalAuthToEntity is the entity to authenticate for after
    // externally authenticating.
    // This is unlikely to be needed since the signaler is typically in the same
    // place where authentication happens.
    string SignalingExternalAuthToEntity = null!,

    // SignalingCredentials are used to authenticate the request to the signaling server.
    Credentials SignalingCredentials = null!,

    string SignalingServerAddress = null!,
    bool SignalingExternalAuthInsecure = false
);
