using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Gripper.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Gripper
{
    internal class GripperService(ILogger<GripperService> logger)
        : Component.Gripper.V1.GripperService.GripperServiceBase, IComponentServiceBase
    {
        public static Service ServiceName => Service.GripperService;
        public static SubType SubType { get; } = SubType.Gripper;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGripper)context.UserState["resource"];
                var res = await resource.DoCommand(request.Command.ToDictionary(),
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

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

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGripper)context.UserState["resource"];
                await resource
                    .Stop(request.Extra?.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken)
                    .ConfigureAwait(false);
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

        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGripper)context.UserState["resource"];
                throw new NotImplementedException();
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
                var resource = (IGripper)context.UserState["resource"];
                var res = await resource.IsMoving(context.Deadline.ToTimeout(), context.CancellationToken)
                    .ConfigureAwait(false);
                var response = new IsMovingResponse() { IsMoving = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GrabResponse> Grab(GrabRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGripper)context.UserState["resource"];
                await resource
                    .Grab(request.Extra?.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken)
                    .ConfigureAwait(false);
                var response = new GrabResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<OpenResponse> Open(OpenRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGripper)context.UserState["resource"];
                await resource
                    .Open(request.Extra?.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken)
                    .ConfigureAwait(false);
                var response = new OpenResponse();
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