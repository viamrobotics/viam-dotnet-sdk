using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Servo.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Servo(ResourceName resourceName, ViamChannel channel) : ComponentBase<Servo, ServoService.ServoServiceClient>(resourceName, new ServoService.ServoServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Servo(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("servo");

        public static Servo FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Servo>(resourceName);
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

        public async ValueTask Move(uint angle,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.MoveAsync(new MoveRequest() { Name = Name, AngleDeg = angle, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask<uint> GetPosition(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPositionAsync(new GetPositionRequest() { Name = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.PositionDeg;
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
    }
}
