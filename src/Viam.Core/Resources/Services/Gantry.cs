using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Gantry.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Gantry : GantryService.GantryServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.gantry.v1.GantryService";


        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            await resource.Stop(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken);
            return new StopResponse();
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetGeometriesResponse() { Geometries = { res } };
        }

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            var res = await resource.IsMoving(context.Deadline.ToTimeout(), context.CancellationToken);
            return new IsMovingResponse() { IsMoving = res };
        }

        public override async Task<GetLengthsResponse> GetLengths(GetLengthsRequest request, ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            var res = await resource.GetLengths(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken);
            return new GetLengthsResponse() { LengthsMm = { res } };
        }

        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request,
                                                                    ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            var res = await resource.GetPosition(request.Extra,
                                                 context.Deadline.ToTimeout(),
                                                 context.CancellationToken);

            return new GetPositionResponse() { PositionsMm = { res } };
        }

        public override async Task<HomeResponse> Home(HomeRequest request, ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            await resource.Home(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken);
            return new HomeResponse();
        }

        public override async Task<MoveToPositionResponse> MoveToPosition(
            MoveToPositionRequest request,
            ServerCallContext context)
        {
            var resource = (IGantry)context.UserState["resource"];
            await resource.MoveToPosition(request.PositionsMm.ToArray(),
                                          request.SpeedsMmPerSec.ToArray(),
                                          request.Extra,
                                          context.Deadline.ToTimeout(),
                                          context.CancellationToken);

            return new MoveToPositionResponse();
        }
    }
}
