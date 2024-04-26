using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Servo.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Servo : ServoService.ServoServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.servo.v1.ServoService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IServo)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IServo)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetGeometriesResponse() { Geometries = { res } };
        }

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            var resource = (IServo)context.UserState["resource"];
            await resource.Stop(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken);
            return new StopResponse();
        }

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            var resource = (IServo)context.UserState["resource"];
            var res = await resource.IsMoving(context.Deadline.ToTimeout(), context.CancellationToken);
            return new IsMovingResponse() { IsMoving = res };
        }

        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request,
                                                                    ServerCallContext context)
        {
            var resource = (IServo)context.UserState["resource"];
            var res = await resource.GetPosition(request.Extra,
                                                 context.Deadline.ToTimeout(),
                                                 context.CancellationToken);

            return new GetPositionResponse() { PositionDeg = res };
        }

        public override async Task<MoveResponse> Move(MoveRequest request, ServerCallContext context)
        {
            var resource = (IServo)context.UserState["resource"];
            await resource.Move(request.AngleDeg,
                                request.Extra,
                                context.Deadline.ToTimeout(),
                                context.CancellationToken);

            return new MoveResponse();
        }
    }
}
