using Grpc.Net.Client;
using Proto.Rpc.V1;

namespace Viam.Net.Sdk.Core
{
    public class DialOptions
    {
        public bool Insecure { get; set; } = false;
        public string AuthEntity { get; set; } = null!;
        public Credentials Credentials { get; set; } = null!;
        public DialWebRTCOptions WebRTCOptions { get; set; } = null!;
        public string ExternalAuthAddress { get; set; } = null!;
        public string ExternalAuthToEntity { get; set; } = null!;
        public bool ExternalAuthInsecure { get; set; } = false;
        public string Authority { get; set; } = null!;
        public GrpcChannelOptions ChannelOptions { get; set; } = null!;

        public DialOptions Clone()
        {
            var dialOpts = (DialOptions)MemberwiseClone();
            dialOpts.Credentials = Credentials?.Clone();
            dialOpts.WebRTCOptions = (DialWebRTCOptions)WebRTCOptions.Clone();
            return dialOpts;
        }
    }
}