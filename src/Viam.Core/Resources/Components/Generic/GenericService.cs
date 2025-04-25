using System;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Generic.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Generic
{
    internal class GenericService(ILogger<GenericService> logger) : Component.Generic.V1.GenericService.GenericServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.generic.v1.GenericService";
        public SubType SubType { get; } = SubType.FromRdkComponent("generic");

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IGeneric)context.UserState["resource"];
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
                var resource = (IGeneric)context.UserState["resource"];
                var res = await resource.GetGeometries(request.Extra?.ToDictionary(),
                                                       context.Deadline.ToTimeout(),
                                                       context.CancellationToken).ConfigureAwait(false);

                var response = new GetGeometriesResponse() { Geometries = { res } };
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
