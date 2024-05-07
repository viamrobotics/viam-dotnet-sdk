using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Powersensor.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class PowerSensor(ILogger<PowerSensor> logger) : PowerSensorService.PowerSensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.powersensor.v1.PowerSensorService";
        public SubType SubType { get; } = SubType.FromRdkComponent("power_sensor");

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IPowerSensor)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context)
        {
            var resource = (IPowerSensor)context.UserState["resource"];
            var res = await resource.GetReadings(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken).ConfigureAwait(false);

            var r = new GetReadingsResponse();
            r.Readings.Add(res);
            return r;
        }

        public override async Task<GetCurrentResponse> GetCurrent(GetCurrentRequest request, ServerCallContext context)
        {
            var resource = (IPowerSensor)context.UserState["resource"];
            var res = await resource.GetCurrent(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new GetCurrentResponse() { Amperes = res.Item1, IsAc = res.Item2 };
        }

        public override async Task<GetPowerResponse> GetPower(GetPowerRequest request, ServerCallContext context)
        {
            var resource = (IPowerSensor)context.UserState["resource"];
            var res = await resource.GetPower(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new GetPowerResponse() { Watts = res };
        }

        public override async Task<GetVoltageResponse> GetVoltage(GetVoltageRequest request, ServerCallContext context)
        {
            var resource = (IPowerSensor)context.UserState["resource"];
            var res = await resource.GetVoltage(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new GetVoltageResponse() { Volts = res.Item1, IsAc = res.Item2 };
        }
    }
}
