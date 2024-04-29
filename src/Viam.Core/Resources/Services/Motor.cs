using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Motor.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Motor(ILogger logger) : MotorService.MotorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.motor.v1.MotorService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken).ConfigureAwait(false);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            await resource.Stop(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new StopResponse();
        }

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            var res = await resource.IsMoving(context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new IsMovingResponse() { IsMoving = res };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                  ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken).ConfigureAwait(false);

            return new GetGeometriesResponse() { Geometries = { res } };
        }

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            var res = await resource.GetProperties(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken).ConfigureAwait(false);

            return new GetPropertiesResponse() { PositionReporting = res.PositionReporting };
        }

        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request,
                                                                    ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            var res = await resource.GetPosition(request.Extra,
                                                 context.Deadline.ToTimeout(),
                                                 context.CancellationToken).ConfigureAwait(false);

            return new GetPositionResponse() { Position = res };
        }

        public override async Task<GoForResponse> GoFor(GoForRequest request, ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            await resource.GoFor(request.Rpm,
                                 request.Revolutions,
                                 request.Extra,
                                 context.Deadline.ToTimeout(),
                                 context.CancellationToken).ConfigureAwait(false);

            return new GoForResponse();
        }

        public override async Task<GoToResponse> GoTo(GoToRequest request, ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            await resource.GoTo(request.Rpm,
                                request.PositionRevolutions,
                                request.Extra,
                                context.Deadline.ToTimeout(),
                                context.CancellationToken).ConfigureAwait(false);

            return new GoToResponse();
        }

        public override async Task<IsPoweredResponse> IsPowered(IsPoweredRequest request, ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            var res = await resource.IsPowered(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new IsPoweredResponse() { IsOn = res.IsOn, PowerPct = res.PowerPct };
        }

        public override async Task<ResetZeroPositionResponse> ResetZeroPosition(
            ResetZeroPositionRequest request,
            ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            await resource.ResetZeroPosition(request.Offset,
                                             request.Extra,
                                             context.Deadline.ToTimeout(),
                                             context.CancellationToken).ConfigureAwait(false);

            return new ResetZeroPositionResponse();
        }

        public override async Task<SetPowerResponse> SetPower(SetPowerRequest request, ServerCallContext context)
        {
            var resource = (IMotor)context.UserState["resource"];
            await resource.SetPower(request.PowerPct,
                                    request.Extra,
                                    context.Deadline.ToTimeout(),
                                    context.CancellationToken).ConfigureAwait(false);

            return new SetPowerResponse();
        }
    }
}
