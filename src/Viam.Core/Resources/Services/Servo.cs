using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Servo.V1;

namespace Viam.Core.Resources.Services
{
    internal class Servo : ServoService.ServoServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.servo.v1.ServoService";
        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request, ServerCallContext context) => base.GetGeometries(request, context);
        public override Task<StopResponse> Stop(StopRequest request, ServerCallContext context) => base.Stop(request, context);
        public override Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context) => base.IsMoving(request, context);
        public override Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context) => base.DoCommand(request, context);
        public override Task<GetPositionResponse> GetPosition(GetPositionRequest request, ServerCallContext context) => base.GetPosition(request, context);
        public override Task<MoveResponse> Move(MoveRequest request, ServerCallContext context) => base.Move(request, context);
    }
}
