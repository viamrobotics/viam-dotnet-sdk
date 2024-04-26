using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Servo.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IServo : IComponentBase
    {
        ValueTask Move(uint angle,
                       Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<uint> GetPosition(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

    }
    public class Servo(ResourceName resourceName, ViamChannel channel) : ComponentBase<Servo, ServoService.ServoServiceClient>(resourceName, new ServoService.ServoServiceClient(channel)), IServo
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Servo(name, channel), manager => new Services.Servo()));
        public static SubType SubType = SubType.FromRdkComponent("servo");

        public static Servo FromRobot(RobotClientBase client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Servo>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
                                                         {
                                                             Name = ResourceName.Name, Command = command.ToStruct()
                                                         });

            return res.Result.ToDictionary();
        }

        public async ValueTask Move(uint angle,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.MoveAsync(new MoveRequest() { Name = Name, AngleDeg = angle, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken);
        }

        public async ValueTask<uint> GetPosition(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPositionAsync(new GetPositionRequest() { Name = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken);

            return res.PositionDeg;
        }

        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = ResourceName.Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken);
            return res.IsMoving;
        }

        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken);
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
    }
}
