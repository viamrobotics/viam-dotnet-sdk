using System;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Encoder
{
    internal class EncoderService(ILogger<EncoderService> logger) : Component.Encoder.V1.EncoderService.EncoderServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.encoder.v1.EncoderService";
        public SubType SubType { get; } = SubType.FromRdkComponent("encoder");

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IEncoder)context.UserState["resource"];
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
                var resource = (IEncoder)context.UserState["resource"];
                var res = await resource.GetGeometries(request.Extra,
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

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                        ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IEncoder)context.UserState["resource"];
                var res = await resource.GetProperties(request.Extra,
                                                       context.Deadline.ToTimeout(),
                                                       context.CancellationToken).ConfigureAwait(false);

                var response = new GetPropertiesResponse()
                {
                    AngleDegreesSupported = res.AngleDegreesSupported,
                    TicksCountSupported = res.TicksCountSupported
                };

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
                var resource = (IEncoder)context.UserState["resource"];
                var res = await resource.GetPosition(request.PositionType,
                                                     request.Extra,
                                                     context.Deadline.ToTimeout(),
                                                     context.CancellationToken).ConfigureAwait(false);

                var response = new GetPositionResponse() { PositionType = res.PositionType, Value = res.Position };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<ResetPositionResponse> ResetPosition(ResetPositionRequest request,
                                                                        ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IEncoder)context.UserState["resource"];
                await resource.ResetPosition(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
                var response = new ResetPositionResponse();
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
