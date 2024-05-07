using System;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.Collections;

using Microsoft.Extensions.Logging;

using Viam.App.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Clients
{
    public class AppClientBase
    {
        protected readonly ILogger<AppClientBase> Logger;
        private readonly AppService.AppServiceClient _appServiceClient;

        protected internal AppClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            Logging.Logger.SetLoggerFactory(loggerFactory);
            Logger = loggerFactory.CreateLogger<AppClientBase>();
            _appServiceClient = new AppService.AppServiceClient(channel);
        }

        [LogInvocation]
        public async Task<RepeatedField<RobotPart>> GetRobotPartsAsync(string robotId, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var result = await _appServiceClient.GetRobotPartsAsync(new GetRobotPartsRequest() { RobotId = robotId },
                                                                    deadline: timeout.ToDeadline(),
                                                                    cancellationToken: cancellationToken)
                                                .ConfigureAwait(false);

            return result.Parts;
        }
    }
}
