using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Servo.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Servo
{
    internal class ServoService(ILogger<ServoService> logger)
        : Component.Servo.V1.ServoService.ServoServiceBase, IComponentServiceBase
    {
        public static Service ServiceName => Service.ServoService;
        public static SubType SubType { get; } = SubType.Servo;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IServo)context.UserState["resource"];
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

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IServo)context.UserState["resource"];
                var res = await resource.GetGeometries(request.Extra?.ToDictionary(),
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetGeometriesResponse() { Geometries = { res } };
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
                var resource = (IServo)context.UserState["resource"];
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

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IServo)context.UserState["resource"];
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

        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IServo)context.UserState["resource"];
                var res = await resource.GetPosition(request.Extra?.ToDictionary(),
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetPositionResponse() { PositionDeg = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<MoveResponse> Move(MoveRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IServo)context.UserState["resource"];
                await resource.Move(request.AngleDeg,
                    request.Extra?.ToDictionary(),
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new MoveResponse();
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