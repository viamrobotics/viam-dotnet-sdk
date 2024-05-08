using System;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Sensor
{
    internal class SensorService(ILogger<SensorService> logger) : Component.Sensor.V1.SensorService.SensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.sensor.v1.SensorService";
        public SubType SubType { get; } = SubType.FromRdkComponent("sensor");

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ISensor)context.UserState["resource"];
                var res = await resource.DoCommand(request.Command.ToDictionary(),
                                                   context.Deadline.ToTimeout(),
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

        public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ISensor)context.UserState["resource"];
                var res = await resource.GetReadings(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);

                var response = new GetReadingsResponse();
                response.Readings.Add(res);
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
