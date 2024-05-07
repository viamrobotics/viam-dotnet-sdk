using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Fody;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Sensor.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface ISensor : IComponentBase
    {
        public ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                                 TimeSpan? timeout = null,
                                                                 CancellationToken cancellationToken = default);
    }

    [ConfigureAwait(false)]
    public class Sensor(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<Sensor, SensorService.SensorServiceClient>(resourceName, new SensorService.SensorServiceClient(channel)),
          ISensor
    {
        static Sensor() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new Sensor(name, channel, logger)));

        public static SubType SubType = SubType.FromRdkComponent("sensor");
        public static ViamResourceName GetResourceName(string? name)
        {
            ArgumentNullException.ThrowIfNull(name, nameof(name));
            return new ViamResourceName(SubType, name);
        }

        [LogInvocation]
        public static Sensor FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Sensor>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        [LogInvocation]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(
                                      new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                                      deadline: timeout.ToDeadline(),
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogInvocation]
        public async ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                                         TimeSpan? timeout = null,
                                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .GetReadingsAsync(new GetReadingsRequest() { Name = ResourceName.Name, Extra = extra },
                                              deadline: timeout.ToDeadline(),
                                              cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Readings.ToDictionary();
        }
    }
}

