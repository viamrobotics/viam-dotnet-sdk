using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Base.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Base : BaseService.BaseServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.base.v1.BaseService";

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            await resource.Stop(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken);
            return new StopResponse();
        }

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                         context.Deadline.ToTimeout(),
                                         context.CancellationToken);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            var res = await resource.IsMoving(context.Deadline.ToTimeout(), context.CancellationToken);
            return new IsMovingResponse() { IsMoving = res };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                  ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetGeometriesResponse() { Geometries = { res } };
        }

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                  ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            var res = await resource.GetProperties(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetPropertiesResponse()
                   {
                       TurningRadiusMeters = res.TurningRadius,
                       WheelCircumferenceMeters = res.WheelCircumference,
                       WidthMeters = res.Width
                   };
        }

        public override async Task<MoveStraightResponse> MoveStraight(MoveStraightRequest request, ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            await resource.MoveStraight(request.DistanceMm,
                                        request.MmPerSec,
                                        request.Extra,
                                        context.Deadline.ToTimeout(),
                                        context.CancellationToken);

            return new MoveStraightResponse();
        }

        public override async Task<SetPowerResponse> SetPower(SetPowerRequest request, ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            await resource.SetPower(request.Linear,
                                    request.Angular,
                                    request.Extra,
                                    context.Deadline.ToTimeout(),
                                    context.CancellationToken);
            return new SetPowerResponse();
        }

        public override async Task<SetVelocityResponse> SetVelocity(SetVelocityRequest request, ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            await resource.SetVelocity(request.Linear,
                                       request.Angular,
                                       request.Extra,
                                       context.Deadline.ToTimeout(),
                                       context.CancellationToken);
            return new SetVelocityResponse();
        }

        public override async Task<SpinResponse> Spin(SpinRequest request, ServerCallContext context)
        {
            var resource = (IBase)context.UserState["resource"];
            await resource.Spin(request.AngleDeg,
                                request.DegsPerSec,
                                request.Extra,
                                context.Deadline.ToTimeout(),
                                context.CancellationToken);

            return new SpinResponse();
        }
    }
}
