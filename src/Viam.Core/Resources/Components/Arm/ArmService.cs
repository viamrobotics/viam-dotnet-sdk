using System;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Arm.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Arm
{
    internal class ArmService(ILogger<ArmService> logger) : Component.Arm.V1.ArmService.ArmServiceBase, IServiceBase
    {
        public static Service ServiceName => Service.ArmService;
        public static SubType SubType { get; } = SubType.Arm;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                var res = await resource.DoCommand(request.Command.ToDictionary(),
                                                   context.Deadline - DateTime.UtcNow,
                                                   context.CancellationToken)
                                        .ConfigureAwait(false);
                var response = new DoCommandResponse() { Result = res.ToStruct() };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetEndPositionResponse> GetEndPosition(GetEndPositionRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                // TODO: This deadline math is probably wrong.
                var pose = await resource
                                 .GetEndPosition(request.Extra?.ToDictionary(),
                                                 context.Deadline - DateTime.UtcNow,
                                                 context.CancellationToken)
                                 .ConfigureAwait(false);
                var response = new GetEndPositionResponse() { Pose = pose };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                var geometries = await resource
                                       .GetGeometries(request.Extra?.ToDictionary(),
                                                      context.Deadline - DateTime.UtcNow,
                                                      context.CancellationToken)
                                       .ConfigureAwait(false);
                var response = new GetGeometriesResponse() { Geometries = { geometries } };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetJointPositionsResponse> GetJointPositions(GetJointPositionsRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                var jointPositions = await resource
                                           .GetJointPositions(request.Extra?.ToDictionary(),
                                                              context.Deadline - DateTime.UtcNow,
                                                              context.CancellationToken)
                                           .ConfigureAwait(false);
                var response = new GetJointPositionsResponse() { Positions = jointPositions };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetKinematicsResponse> GetKinematics(GetKinematicsRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                var (format, data) = await resource.GetKinematics(request.Extra?.ToDictionary(), context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
                var response = new GetKinematicsResponse() { Format = format, KinematicsData = data };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                var isMoving = await resource.IsMoving(context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
                var response = new IsMovingResponse() { IsMoving = isMoving };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<MoveToJointPositionsResponse> MoveToJointPositions(MoveToJointPositionsRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                await resource.MoveToJoinPositions(request.Positions, request.Extra?.ToDictionary(), context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
                var response = new MoveToJointPositionsResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<MoveToPositionResponse> MoveToPosition(MoveToPositionRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                await resource.MoveToPosition(request.To, request.Extra?.ToDictionary(), context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
                var response = new MoveToPositionResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IArm)context.UserState["resource"];
                await resource.Stop(request.Extra?.ToDictionary(), context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);
                var response = new StopResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}
