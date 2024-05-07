using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Powersensor.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IPowerSensor : ISensor
    {
        ValueTask<(double, bool)> GetVoltage(Struct? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default);

        ValueTask<(double, bool)> GetCurrent(Struct? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default);

        ValueTask<double> GetPower(Struct? extra = null,
                                   TimeSpan? timeout = null,
                                   CancellationToken cancellationToken = default);
    }
    public class PowerSensor(ViamResourceName resourceName, ViamChannel channel, ILogger logger) :
        ComponentBase<PowerSensor, PowerSensorService.PowerSensorServiceClient>(resourceName, new PowerSensorService.PowerSensorServiceClient(channel)),
        IPowerSensor
    {
        static PowerSensor() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new PowerSensor(name, channel, logger)));
        public static SubType SubType = SubType.FromRdkComponent("power_sensor");

        [LogInvocation]
        public static PowerSensor FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<PowerSensor>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        [LogInvocation]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .DoCommandAsync(
                                new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                                deadline: timeout.ToDeadline(),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogInvocation]
        public async ValueTask<(double, bool)> GetVoltage(Struct? extra = null,
                                                          TimeSpan? timeout = null,
                                                          CancellationToken cancellationToken = default)
        {
            var res = await Client.GetVoltageAsync(new GetVoltageRequest() { Name = Name, Extra = extra },
                                                   deadline: timeout.ToDeadline(),
                                                   cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return (res.Volts, res.IsAc);
        }

        [LogInvocation]
        public async ValueTask<(double, bool)> GetCurrent(Struct? extra = null,
                                                          TimeSpan? timeout = null,
                                                          CancellationToken cancellationToken = default)
        {
            var res = await Client.GetCurrentAsync(new GetCurrentRequest() { Name = Name, Extra = extra },
                                                   deadline: timeout.ToDeadline(),
                                                   cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return (res.Amperes, res.IsAc);
        }

        [LogInvocation]
        public async ValueTask<double> GetPower(Struct? extra = null,
                                                TimeSpan? timeout = null,
                                                CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPowerAsync(new GetPowerRequest() { Name = Name, Extra = extra },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Watts;
        }

        [LogInvocation]
        public async ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                                         TimeSpan? timeout = null,
                                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetReadingsAsync(new GetReadingsRequest() { Name = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Readings.ToDictionary();
        }
    }
}
