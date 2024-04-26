using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Viam.Common.V1;
using Viam.Component.Motor.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IMotor : IResourceBase
    {
        ValueTask SetPower(double power,
                           Struct? extra = null,
                           TimeSpan? timeout = null,
                           CancellationToken cancellationToken = default);

        ValueTask GoFor(double rpm,
                      double revolutions,
                      Struct? extra = null,
                      TimeSpan? timeout = null,
                      CancellationToken cancellationToken = default);

        ValueTask GoTo(double rpm,
                       double positionRevolutions,
                       Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask ResetZeroPosition(double offset,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default);

        ValueTask<double> GetPosition(Struct? extra = null,
                                      TimeSpan? timeout = null,
                                      CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<(bool IsOn, double PowerPct)> IsPowered(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask<Motor.Properties> GetProperties(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }

    public class Motor(ResourceName resourceName, ViamChannel channel) : ComponentBase<Motor, MotorService.MotorServiceClient>(resourceName, new MotorService.MotorServiceClient(channel)), IMotor
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Motor(name, channel), manager => new Services.Motor()));
        public static SubType SubType = SubType.FromRdkComponent("motor");

        public static Motor FromRobot(RobotClientBase client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Motor>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
            {
                Name = ResourceName.Name,
                Command = command.ToStruct()
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
                                       cancellationToken: cancellationToken);
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
                cancellationToken: cancellationToken);
        }

        public async ValueTask GoTo(double rpm,
                                    double positionRevolutions,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.GoToAsync(new GoToRequest()
            {
                Name = Name,
                Rpm = rpm,
                PositionRevolutions = positionRevolutions,
                Extra = extra
            },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken);
        }

        public async ValueTask ResetZeroPosition(double offset,
                                                 Struct? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default)
        {
            await Client.ResetZeroPositionAsync(
                            new ResetZeroPositionRequest() { Name = Name, Offset = offset, Extra = extra },
                            deadline: timeout.ToDeadline(),
                            cancellationToken: cancellationToken);
        }

        public async ValueTask<double> GetPosition(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPositionAsync(new GetPositionRequest() { Name = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken);

            return res.Position;
        }

        public async ValueTask<Properties> GetProperties(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken);

            return new Properties(res.PositionReporting);
        }

        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken);
        }

        public async ValueTask<(bool IsOn, double PowerPct)> IsPowered(Struct? extra = null,
                                               TimeSpan? timeout = null,
                                               CancellationToken cancellationToken = default)
        {
            var res = await Client.IsPoweredAsync(new IsPoweredRequest() { Name = Name, Extra = extra },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken);

            return (res.IsOn, res.PowerPct);
        }

        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken);
            return res.IsMoving;
        }

        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken);

            return res.Geometries.ToArray();
        }

        public record Properties(bool PositionReporting);
    }
}
