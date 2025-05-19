using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Gantry.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Gantry
{
    internal class GantryService(ILogger<GantryService> logger)
        : Component.Gantry.V1.GantryService.GantryServiceBase, IServiceBase
    {
        public static Service ServiceName => Service.GantryService;
        public static SubType SubType { get; } = SubType.Gantry;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGantry)context.UserState["resource"];
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
                var resource = (IGantry)context.UserState["resource"];
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

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGantry)context.UserState["resource"];
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

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGantry)context.UserState["resource"];
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

        public override async Task<GetLengthsResponse> GetLengths(GetLengthsRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGantry)context.UserState["resource"];
                var res = await resource.GetLengths(request.Extra?.ToDictionary(), context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);
                var response = new GetLengthsResponse() { LengthsMm = { res } };
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
                var resource = (IGantry)context.UserState["resource"];
                var res = await resource.GetPosition(request.Extra?.ToDictionary(),
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetPositionResponse() { PositionsMm = { res } };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<HomeResponse> Home(HomeRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGantry)context.UserState["resource"];
                await resource
                    .Home(request.Extra?.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken)
                    .ConfigureAwait(false);
                var response = new HomeResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<MoveToPositionResponse> MoveToPosition(
            MoveToPositionRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGantry)context.UserState["resource"];
                await resource.MoveToPosition(request.PositionsMm.ToArray(),
                    request.SpeedsMmPerSec.ToArray(),
                    request.Extra?.ToDictionary(),
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

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
    }
}