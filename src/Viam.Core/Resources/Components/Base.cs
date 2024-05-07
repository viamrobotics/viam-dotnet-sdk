using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Base.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IBase : IComponentBase
    {
        /// <summary>
        /// Move straight
        /// </summary>
        /// <param name="distance">The distance to move in mm</param>
        /// <param name="velocity">The velocity to move at in mm/sec</param>
        /// <param name="extra"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask MoveStraight(long distance,
                               double velocity,
                               Struct? extra = null,
                               TimeSpan? timeout = null,
                               CancellationToken cancellationToken = default);

        /// <summary>
        /// Spin the base
        /// </summary>
        /// <param name="angle">The angle to rotate in degrees</param>
        /// <param name="velocity">The velocity to spin in degrees/sec</param>
        /// <param name="extra"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask Spin(double angle,
                       double velocity,
                       Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the power of all motors on the base
        /// </summary>
        /// <param name="linear">The linear <see cref="Vector3"/></param>
        /// <param name="angular">The angular <see cref="Vector3"/></param>
        /// <param name="extra"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask SetPower(Vector3 linear,
                           Vector3 angular,
                           Struct? extra = null,
                           TimeSpan? timeout = null,
                           CancellationToken cancellationToken = default);

        ValueTask SetVelocity(Vector3 linear,
                              Vector3 angular,
                              Struct? extra = null,
                              TimeSpan? timeout = null,
                              CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask<Base.Properties> GetProperties(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }

    public class Base(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<Base, BaseService.BaseServiceClient>(resourceName, new BaseService.BaseServiceClient(channel)),
          IBase
    {
        static Base() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new Base(name, channel, logger)));
        public static SubType SubType = SubType.FromRdkComponent("base");

        [LogInvocation]
        public static Board FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Board>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        [LogInvocation]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogInvocation]
        public async ValueTask MoveStraight(long distance,
                                            double velocity,
                                            Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default)
        {
            await Client.MoveStraightAsync(
                            new MoveStraightRequest()
                            {
                                Name = Name,
                                DistanceMm = distance,
                                MmPerSec = velocity,
                                Extra = extra
                            },
                            deadline: timeout.ToDeadline(),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask Spin(double angle,
                                    double velocity,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.SpinAsync(new SpinRequest()
            {
                Name = Name,
                AngleDeg = angle,
                DegsPerSec = velocity,
                Extra = extra
            },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask SetPower(Vector3 linear,
                                        Vector3 angular,
                                        Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default)
        {
            await Client
                  .SetPowerAsync(
                      new SetPowerRequest() { Name = Name, Linear = linear, Angular = angular, Extra = extra },
                      deadline: timeout.ToDeadline(),
                      cancellationToken: cancellationToken)
                  .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask SetVelocity(Vector3 linear,
                                           Vector3 angular,
                                           Struct? extra = null,
                                           TimeSpan? timeout = null,
                                           CancellationToken cancellationToken = default)
        {
            await Client.SetVelocityAsync(new SetVelocityRequest()
            {
                Name = Name,
                Linear = linear,
                Angular = angular,
                Extra = extra
            },
                                          deadline: timeout.ToDeadline(),
                                          cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.IsMoving;
        }

        [LogInvocation]
        public async ValueTask<Properties> GetProperties(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return new Properties(res.TurningRadiusMeters, res.WheelCircumferenceMeters, res.WidthMeters);
        }

        [LogInvocation]
        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra },
                                                deadline: timeout.ToDeadline(),
                                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Geometries.ToArray();
        }

        /// <summary>
        /// Various dimensions of the base
        /// </summary>
        /// <param name="TurningRadius">The turning radius of the base, in meters</param>
        /// <param name="WheelCircumference">The circumference of the wheels on the base, in meters. It is assumed all wheels have the same circumference</param>
        /// <param name="Width">The width of the base, in meters</param>
        public record Properties(double TurningRadius, double WheelCircumference, double Width);
    }
}
