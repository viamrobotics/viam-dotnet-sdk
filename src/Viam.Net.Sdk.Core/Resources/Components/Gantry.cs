using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Gantry.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IGantry : IComponentBase
    {
        ValueTask<double[]> GetPosition(Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default);

        ValueTask MoveToPosition(double[] positions,
                                 double[] speeds,
                                 Struct? extra = null,
                                 TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask Home(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<double[]> GetLengths(Struct? extra = null,
                                       TimeSpan? timeout = null,
                                       CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }
    public class Gantry(ResourceName resourceName, ViamChannel channel) : ComponentBase<Gantry, GantryService.GantryServiceClient>(resourceName, new GantryService.GantryServiceClient(channel)), IGantry
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Gantry(name, channel)));
        public static SubType SubType = SubType.FromRdkComponent("gantry");
        public static Gantry FromRobot(RobotClient client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Gantry>(resourceName);
        }

        public override DateTime? LastReconfigured => null;
        
        internal override ValueTask StopResource() => Stop();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object> command,
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
