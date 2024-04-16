using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using Viam.App.V1;

namespace Viam.Net.Sdk.Core.Clients
{
    public class AppClient : ViamClient
    {
        private readonly AppService.AppServiceClient _appServiceClient;
        protected internal AppClient(ILogger logger, ViamChannel channel) : base(logger, channel)
        {
            _appServiceClient = new AppService.AppServiceClient(channel);
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
