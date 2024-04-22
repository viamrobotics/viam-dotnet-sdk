using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Gantry.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Gantry(ResourceName resourceName, ViamChannel channel) : ComponentBase<Gantry, GantryService.GantryServiceClient>(resourceName, new GantryService.GantryServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Gantry(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("gantry");
        public static Gantry FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Gantry>(resourceName);
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

        public async ValueTask<double[]> GetPosition(Struct? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPositionAsync(new GetPositionRequest() { Name = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.PositionsMm.ToArray();
        }

        public async ValueTask MoveToPosition(double[] positions,
                                              double[] speeds,
                                              Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            await Client.MoveToPositionAsync(new MoveToPositionRequest()
                                             {
                                                 Name = Name,
                                                 PositionsMm = { positions },
                                                 SpeedsMmPerSec = { speeds },
                                                 Extra = extra
                                             },
                                             deadline: timeout.ToDeadline(),
                                             cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask Home(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.HomeAsync(new HomeRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask<double[]> GetLengths(Struct? extra = null,
                                          TimeSpan? timeout = null,
                                          CancellationToken cancellationToken = default)
        {
            var res = await Client.GetLengthsAsync(new GetLengthsRequest() { Name = Name, Extra = extra },
                                         deadline: timeout.ToDeadline(),
                                         cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

            return res.LengthsMm.ToArray();
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
    }
}
