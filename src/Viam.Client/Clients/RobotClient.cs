using Microsoft.Extensions.Logging;

using Viam.Client.Dialing;
using Viam.Client.Options;
using Viam.Core;
using Viam.Core.Clients;
using Viam.Core.Resources;
using Viam.Robot.V1;

namespace Viam.Client.Clients
{
    public sealed class RobotClient : RobotClientBase
    {
        private readonly RobotService.RobotServiceClient _robotServiceClient;
        private readonly ResourceManager _resourceManager;

        private RobotClient(ILoggerFactory loggerFactory, ViamChannel channel)
            : base(loggerFactory, channel)
        {
            _robotServiceClient = new RobotService.RobotServiceClient(channel);
            _resourceManager = new ResourceManager(loggerFactory);
        }

        public static async ValueTask<RobotClient> AtAddressAsync(ViamClientOptions options)
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
