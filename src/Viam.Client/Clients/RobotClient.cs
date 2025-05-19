using Microsoft.Extensions.Logging;
using Viam.Client.Dialing;
using Viam.Core;
using Viam.Core.Clients;

namespace Viam.Client.Clients
{
    /// <summary>
    /// A client for interacting with Viam Smart Machines
    /// </summary>
    public sealed class RobotClient : RobotClientBase
    {
        private RobotClient(ILoggerFactory loggerFactory, ViamChannel channel)
            : base(loggerFactory, channel)
        {
        }

        public static async ValueTask<RobotClient> AtAddressAsync(DialOptions options)
        {
            var dialer = new Dialer(options.LoggerFactory);
            var channel = options.DisableWebRtc
                ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            var client = new RobotClient(options.LoggerFactory, channel);
            await client.RefreshAsync();
            return client;
        }

        public static async ValueTask<RobotClient> WithChannel(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            var client = new RobotClient(loggerFactory, channel);
            await client.RefreshAsync();
            return client;
        }
    }
}