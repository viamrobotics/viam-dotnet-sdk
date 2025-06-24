using Fody;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viam.Common.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;
using Viam.Serialization;

namespace Viam.Core.Resources.Components.Sensor
{
    [ConfigureAwait(false)]
    public class SensorClient(ViamResourceName resourceName, ViamChannel channel, ILogger<SensorClient> logger)
        : ComponentBase<SensorClient, Component.Sensor.V1.SensorService.SensorServiceClient>(
                resourceName,
                new Component.Sensor.V1.SensorService.SensorServiceClient(channel)),
            ISensor
    {
        public static SubType SubType = SubType.FromRdkComponent("sensor");

        public static ViamResourceName GetResourceName(string? name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return new ViamResourceName(SubType, name);
        }

        public static ISensor FromRobot(MachineClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<SensorClient>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => new ValueTask();

        public override async ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(
                        new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result.ToDictionary();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Dictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                    .GetReadingsAsync(new GetReadingsRequest() { Name = ResourceName.Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Readings.ToDictionary();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}