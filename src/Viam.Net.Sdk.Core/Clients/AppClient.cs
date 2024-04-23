using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using Viam.App.V1;
using Viam.Core.Dialing;
using Viam.Core.Options;

namespace Viam.Core.Clients
{
    public class AppClient
    {
        private readonly AppService.AppServiceClient _appServiceClient;
        protected internal AppClient(ILogger logger, ViamChannel channel)
        {
            _appServiceClient = new AppService.AppServiceClient(channel);
        }

        public static async ValueTask<AppClient> AtAddressAsync(ViamClientOptions options)
        {
            var dialer = new Dialer(options.Logger);
            var channel = options.DisableWebRtc
                              ? await dialer.DialGrpcDirectAsync(options.ToGrpcDialOptions())
                              : await dialer.DialWebRtcDirectAsync(options.ToWebRtcDialOptions());
            var client = new AppClient(options.Logger, channel);
            return client;
        }

        public static ValueTask<AppClient> WithChannel(ILogger logger, ViamChannel channel)
        {
            var client = new AppClient(logger, channel);
            return new ValueTask<AppClient>(client);
        }

        public async Task<RepeatedField<RobotPart>> GetRobotPartsAsync(string robotId)
        {
            var result = await _appServiceClient.GetRobotPartsAsync(new GetRobotPartsRequest() { RobotId = robotId })
                                                .ConfigureAwait(false);

            return result.Parts;
        }

        public RepeatedField<RobotPart> GetRobotParts(string robotId)
        {
            return _appServiceClient.GetRobotParts(new GetRobotPartsRequest() { RobotId = robotId }).Parts;
        }
    }
}
