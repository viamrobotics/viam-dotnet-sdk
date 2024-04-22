using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Powersensor.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class PowerSensor(ResourceName resourceName, ViamChannel channel) : ComponentBase<PowerSensor, PowerSensorService.PowerSensorServiceClient>(resourceName, new PowerSensorService.PowerSensorServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new PowerSensor(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("power_sensor");

        public static PowerSensor FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<PowerSensor>(resourceName);
        }

        public override async ValueTask<IDictionary<string, object?>> DoCommandAsync(IDictionary<string, object> command,
            TimeSpan? timeout = null)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
                                                         {
                                                             Name = ResourceName.Name, Command = command.ToStruct()
                                                         });

            return res.Result.ToDictionary();
        }

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
