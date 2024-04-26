using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Motor.V1;

namespace Viam.Core.Resources.Services
{
    internal class Motor : MotorService.MotorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.motor.v1.MotorService";
        public override Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context) => base.DoCommand(request, context);
        public override Task<StopResponse> Stop(StopRequest request, ServerCallContext context) => base.Stop(request, context);
        public override Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context) => base.IsMoving(request, context);
        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request, ServerCallContext context) => base.GetGeometries(request, context);
        public override Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request, ServerCallContext context) => base.GetProperties(request, context);
        public override Task<GetPositionResponse> GetPosition(GetPositionRequest request, ServerCallContext context) => base.GetPosition(request, context);
        public override Task<GoForResponse> GoFor(GoForRequest request, ServerCallContext context) => base.GoFor(request, context);
        public override Task<GoToResponse> GoTo(GoToRequest request, ServerCallContext context) => base.GoTo(request, context);
        public override Task<IsPoweredResponse> IsPowered(IsPoweredRequest request, ServerCallContext context) => base.IsPowered(request, context);
        public override Task<ResetZeroPositionResponse> ResetZeroPosition(ResetZeroPositionRequest request, ServerCallContext context) => base.ResetZeroPosition(request, context);
        public override Task<SetPowerResponse> SetPower(SetPowerRequest request, ServerCallContext context) => base.SetPower(request, context);
    }
}
