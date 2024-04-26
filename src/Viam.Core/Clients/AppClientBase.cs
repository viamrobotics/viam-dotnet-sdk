using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using Viam.App.V1;

namespace Viam.Core.Clients
{
    public class AppClientBase
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly AppService.AppServiceClient _appServiceClient;
        protected internal AppClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            _loggerFactory = loggerFactory;
            _appServiceClient = new AppService.AppServiceClient(channel);
        }

        public async Task<RepeatedField<RobotPart>> GetRobotPartsAsync(string robotId)
        {
            var result = await _appServiceClient.GetRobotPartsAsync(new GetRobotPartsRequest() { RobotId = robotId })
                                                ;

            return result.Parts;
        }

        public RepeatedField<RobotPart> GetRobotParts(string robotId)
        {
            return _appServiceClient.GetRobotParts(new GetRobotPartsRequest() { RobotId = robotId }).Parts;
        }
    }
}
