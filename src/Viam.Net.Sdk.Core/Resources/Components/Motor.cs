using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Motor.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Motor(ResourceName resourceName, ViamChannel channel) : ComponentBase<Motor, MotorService.MotorServiceClient>(resourceName, new MotorService.MotorServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Motor(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("motor");

        public static Motor FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Motor>(resourceName);
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

        public async ValueTask SetPower(double power,
                                   Struct? extra = null,
                                   TimeSpan? timeout = null,
                                   CancellationToken cancellationToken = default)
        {
            await Client.SetPowerAsync(new SetPowerRequest() { Name = Name, PowerPct = power, Extra = extra },
                                       deadline: timeout.ToDeadline(),
                                       cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask GoFor(double rpm,
                                     double revolutions,
                                     Struct? extra = null,
                                     TimeSpan? timeout = null,
                                     CancellationToken cancellationToken = default)
        {
            await Client.GoForAsync(
                            new GoForRequest() { Name = Name, Revolutions = revolutions, Rpm = rpm, Extra = extra },
                            deadline: timeout.ToDeadline(),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask GoTo(double rpm,
                                    double positionRevolutions,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.GoToAsync(new GoToRequest()
                                   {
                                       Name = Name, Rpm = rpm, PositionRevolutions = positionRevolutions, Extra = extra
                                   },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask ResetZeroPosition(double offset,
                                                 Struct? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default)
        {
            await Client.ResetZeroPositionAsync(
                            new ResetZeroPositionRequest() { Name = Name, Offset = offset, Extra = extra },
                            deadline: timeout.ToDeadline(),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask<double> GetPosition(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPositionAsync(new GetPositionRequest() { Name = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Position;
        }

        public async ValueTask<Properties> GetProperties(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return new Properties(res.PositionReporting);
        }

        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask<(bool, double)> IsPowered(Struct? extra = null,
                                               TimeSpan? timeout = null,
                                               CancellationToken cancellationToken = default)
        {
            var res = await Client.IsPoweredAsync(new IsPoweredRequest() { Name = Name, Extra = extra },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return (res.IsOn, res.PowerPct);
        }

        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
            return res.IsMoving;
        }

        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Geometries.ToArray();
        }

        public record Properties(bool PositionReporting);
    }
}
