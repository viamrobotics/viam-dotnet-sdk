using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Motor.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
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

    public class Motor(ViamResourceName resourceName, ViamChannel channel, ILogger logger) :
        ComponentBase<Motor, MotorService.MotorServiceClient>(resourceName, new MotorService.MotorServiceClient(channel)),
        IMotor
    {
        internal static void RegisterType() => Registry.RegisterSubtype(
            new ResourceRegistration(SubType,
                                            (name, channel, logger) => new Motor(name, channel, logger),
                                            (logger) => new Services.Motor(logger)));
        public static SubType SubType = SubType.FromRdkComponent("motor");

        [LogCall]
        public static Motor FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Motor>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        [LogCall]
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

        [LogCall]
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

        [LogCall]
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

        [LogCall]
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
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogCall]
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

        [LogCall]
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

        [LogCall]
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

        [LogCall]
        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogCall]
        public async ValueTask<(bool IsOn, double PowerPct)> IsPowered(Struct? extra = null,
                                               TimeSpan? timeout = null,
                                               CancellationToken cancellationToken = default)
        {
            var res = await Client.IsPoweredAsync(new IsPoweredRequest() { Name = Name, Extra = extra },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return (res.IsOn, res.PowerPct);
        }

        [LogCall]
        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
            return res.IsMoving;
        }

        [LogCall]
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
