using System;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Component.Arm.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Arm(ILogger logger) : ArmService.ArmServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.arm.v1.ArmService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline - DateTime.UtcNow,
                                               context.CancellationToken)
                                    .ConfigureAwait(false);
            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetEndPositionResponse> GetEndPosition(GetEndPositionRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            // TODO: This deadline math is probably wrong.
            var pose = await resource
                             .GetEndPosition(request.Extra,
                                             context.Deadline - DateTime.UtcNow,
                                             context.CancellationToken)
                             .ConfigureAwait(false);
            return new GetEndPositionResponse() { Pose = pose };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            var geometries = await resource
                                   .GetGeometries(request.Extra,
                                                  context.Deadline - DateTime.UtcNow,
                                                  context.CancellationToken)
                                   .ConfigureAwait(false);
            return new GetGeometriesResponse() { Geometries = { geometries } };
        }

        public override async Task<GetJointPositionsResponse> GetJointPositions(GetJointPositionsRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            var jointPositions = await resource
                                       .GetJointPositions(request.Extra,
                                                          context.Deadline - DateTime.UtcNow,
                                                          context.CancellationToken)
                                       .ConfigureAwait(false);
            return new GetJointPositionsResponse() { Positions = jointPositions };
        }

        public override async Task<GetKinematicsResponse> GetKinematics(GetKinematicsRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            var (format, data) = await resource.GetKinematics(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
            return new GetKinematicsResponse() { Format = format, KinematicsData = data };
        }

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            var isMoving = await resource.IsMoving(context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
            return new IsMovingResponse() { IsMoving = isMoving };
        }

        public override async Task<MoveToJointPositionsResponse> MoveToJointPositions(MoveToJointPositionsRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            await resource.MoveToJoinPositions(request.Positions, request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
            return new MoveToJointPositionsResponse();
        }

        public override async Task<MoveToPositionResponse> MoveToPosition(MoveToPositionRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            await resource.MoveToPosition(request.To, request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
            return new MoveToPositionResponse();
        }

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            var resource = (IArm)context.UserState["resource"];
            await resource.Stop(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
            return new StopResponse();
        }
    }
}
