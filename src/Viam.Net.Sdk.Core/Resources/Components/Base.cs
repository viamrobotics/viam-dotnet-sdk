using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Base.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IBase : IComponentBase
    {
        ValueTask MoveStraight(int distance,
                               float velocity,
                               Struct? extra = null,
                               TimeSpan? timeout = null,
                               CancellationToken cancellationToken = default);

        ValueTask Spin(float angle,
                       float velocity,
                       Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

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

    public class Base(ResourceName resourceName, ViamChannel channel) : ComponentBase<Base, BaseService.BaseServiceClient>(resourceName, new BaseService.BaseServiceClient(channel)), IBase
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Base(name, channel)));
        public static SubType SubType = SubType.FromRdkComponent("base");

        public static Board FromRobot(RobotClient client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Board>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        internal override ValueTask StopResource() => Stop();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object> command,
            TimeSpan? timeout = null)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
                                                         {
                                                             Name = Name, Command = command.ToStruct()
                                                         });

            return res.Result.ToDictionary();
        }

        public async ValueTask MoveStraight(int distance,
                                       float velocity,
                                       Struct? extra = null,
                                       TimeSpan? timeout = null,
                                       CancellationToken cancellationToken = default)
        {
            await Client.MoveStraightAsync(
                new MoveStraightRequest()
                {
                    Name = Name, DistanceMm = distance, MmPerSec = velocity, Extra = extra
                },
                deadline: timeout.ToDeadline(),
                cancellationToken: cancellationToken);
        }

        public async ValueTask Spin(float angle,
                                    float velocity,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.SpinAsync(new SpinRequest()
                                   {
                                       Name = Name, AngleDeg = angle, DegsPerSec = velocity, Extra = extra
                                   },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask SetPower(Vector3 linear,
                                        Vector3 angular,
                                        Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default)
        {
            await Client
                  .SetPowerAsync(
                      new SetPowerRequest()
                      {
                          Name = Name, Linear = linear, Angular = angular, Extra = extra
                      },
                      deadline: timeout.ToDeadline(),
                      cancellationToken: cancellationToken)
                  .ConfigureAwait(false);
        }

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

        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name })
                                  .ConfigureAwait(false);

            return res.IsMoving;
        }

        public async ValueTask<Properties> GetProperties(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra })
                            .ConfigureAwait(false);

            return new Properties(res.TurningRadiusMeters, res.WheelCircumferenceMeters, res.WidthMeters);
        }

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
