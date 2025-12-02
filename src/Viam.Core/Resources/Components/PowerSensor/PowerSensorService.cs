using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Powersensor.V1;
using Viam.Core.Logging;
using Viam.Contracts;
using Viam.Contracts.Resources;

namespace Viam.Core.Resources.Components.PowerSensor
{
    internal class PowerSensorService(ILogger<PowerSensorService> logger)
        : Component.Powersensor.V1.PowerSensorService.PowerSensorServiceBase, IComponentServiceBase
    {
        public static Service ServiceName => Service.PowerSensorService;
        public static SubType SubType { get; } = SubType.PowerSensor;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IPowerSensor)context.UserState["resource"];
                var res = await resource.DoCommand(request.Command, context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);
                var response = new DoCommandResponse() { Result = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IPowerSensor)context.UserState["resource"];
                var res = await resource.GetReadings(request.Extra, context.Deadline - DateTime.UtcNow,
                    context.CancellationToken).ConfigureAwait(false);

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

        public override async Task<GetCurrentResponse> GetCurrent(GetCurrentRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IPowerSensor)context.UserState["resource"];
                var res = await resource.GetCurrent(request.Extra, context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);
                var response = new GetCurrentResponse() { Amperes = res.Item1, IsAc = res.Item2 };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetPowerResponse> GetPower(GetPowerRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IPowerSensor)context.UserState["resource"];
                var res = await resource
                    .GetPower(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken)
                    .ConfigureAwait(false);
                var response = new GetPowerResponse() { Watts = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetVoltageResponse> GetVoltage(GetVoltageRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IPowerSensor)context.UserState["resource"];
                var res = await resource.GetVoltage(request.Extra, context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);
                var response = new GetVoltageResponse() { Volts = res.Item1, IsAc = res.Item2 };
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