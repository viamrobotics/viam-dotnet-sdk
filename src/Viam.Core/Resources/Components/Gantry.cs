using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Gantry.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
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
    public class Gantry(ViamResourceName resourceName, ViamChannel channel, ILogger logger) : 
        ComponentBase<Gantry, GantryService.GantryServiceClient>(resourceName, new GantryService.GantryServiceClient(channel)), 
        IGantry
    {
        internal static void RegisterType() => Registry.RegisterSubtype(
            new ResourceRegistration(SubType,
                                     (name, channel, logger) => new Gantry(name, channel, logger),
                                     (logger) => new Services.Gantry(logger)));
        public static SubType SubType = SubType.FromRdkComponent("gantry");

        [LogCall]
        public static Gantry FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Gantry>(resourceName);
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

        [LogCall]
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

        [LogCall]
        public async ValueTask Home(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.HomeAsync(new HomeRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogCall]
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
    }
}
