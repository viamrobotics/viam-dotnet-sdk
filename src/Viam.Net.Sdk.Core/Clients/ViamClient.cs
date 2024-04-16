using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Viam.Net.Sdk.Core.Dialing;

namespace Viam.Net.Sdk.Core.Clients
{
    public class ViamClient(ILogger logger, ViamChannel channel)
    {
        public static async Task<ViamClient> AtAddressAsync(ViamClientOptions options)
        {
            var dialer = new Dialer(options.Logger);
            var channel = options.DisableWebRtc
                              ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                              : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            return new ViamClient(options.Logger, channel);
        }

        public static ViamClient WithChannel(ILogger logger, ViamChannel channel) => new(logger, channel);

        public RobotClient RobotClient => new(logger, channel);
        public AppClient AppClient => new(logger, channel);
    }
}
