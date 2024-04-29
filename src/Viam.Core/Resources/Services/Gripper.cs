using System;
using System.Threading.Tasks;

using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Gripper.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Gripper(ILogger logger) : GripperService.GripperServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.gripper.v1.GripperService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IGripper)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken).ConfigureAwait(false);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            var resource = (IGripper)context.UserState["resource"];
            await resource.Stop(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new StopResponse();
        }
        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request, ServerCallContext context)
        {
            var resource = (IGripper)context.UserState["resource"];
            throw new NotImplementedException();
        }
        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            var resource = (IGripper)context.UserState["resource"];
            var res = await resource.IsMoving(context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new IsMovingResponse() { IsMoving = res };
        }
        public override async Task<GrabResponse> Grab(GrabRequest request, ServerCallContext context)
        {
            var resource = (IGripper)context.UserState["resource"];
            await resource.Grab(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new GrabResponse();
        }
        public override async Task<OpenResponse> Open(OpenRequest request, ServerCallContext context)
        {
            var resource = (IGripper)context.UserState["resource"];
            await resource.Open(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new OpenResponse();
        }
    }
}
