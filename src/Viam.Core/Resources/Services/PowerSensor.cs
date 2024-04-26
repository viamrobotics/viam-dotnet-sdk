using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Powersensor.V1;

namespace Viam.Core.Resources.Services
{
    internal class PowerSensor : PowerSensorService.PowerSensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.powersensor.v1.PowerSensorService";
        public override Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context) => base.DoCommand(request, context);
        public override Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context) => base.GetReadings(request, context);
        public override Task<GetCurrentResponse> GetCurrent(GetCurrentRequest request, ServerCallContext context) => base.GetCurrent(request, context);
        public override Task<GetPowerResponse> GetPower(GetPowerRequest request, ServerCallContext context) => base.GetPower(request, context);
        public override Task<GetVoltageResponse> GetVoltage(GetVoltageRequest request, ServerCallContext context) => base.GetVoltage(request, context);
    }
}
