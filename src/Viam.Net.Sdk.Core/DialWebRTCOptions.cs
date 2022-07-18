
using Proto.Rpc.V1;
using SIPSorcery.Net;

namespace Viam.Net.Sdk.Core
{
    public class DialWebRTCOptions
    {
        public bool SignalingInsecure { get; set; } = false;
        public bool DisableTrickleICE { get; set; } = false;

        public RTCConfiguration RTCConfig { get; set; } = null!;

        // SignalingAuthEntity is the entity to authenticate as to the signaler.
        public string SignalingAuthEntity { get; set; } = null!;

        // SignalingExternalAuthAddress is the address to perform external auth yet.
        // This is unlikely to be needed since the signaler is typically in the same
        // place where authentication happens.
        public string SignalingExternalAuthAddress { get; set; } = null!;

        // SignalingExternalAuthToEntity is the entity to authenticate for after
        // externally authenticating.
        // This is unlikely to be needed since the signaler is typically in the same
        // place where authentication happens.
        public string SignalingExternalAuthToEntity { get; set; } = null!;

        // SignalingCredentials are used to authenticate the request to the signaling server.
        public Credentials SignalingCredentials { get; set; } = null!;

        public string SignalingServerAddress { get; set; } = null!;
        public bool SignalingExternalAuthInsecure { get; set; } = false;

        public DialWebRTCOptions Clone()
        {
            var dialOpts = (DialWebRTCOptions)MemberwiseClone();
            dialOpts.SignalingCredentials = SignalingCredentials?.Clone();
            return dialOpts;
        }
    }
}