using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Encoder : EncoderService.EncoderServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.encoder.v1.EncoderService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetGeometriesResponse() { Geometries = { res }};
        }

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.GetProperties(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetPropertiesResponse()
                   {
                       AngleDegreesSupported = res.AngleDegreesSupported, TicksCountSupported = res.TicksCountSupported
                   };
        }

        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request,
                                                                    ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.GetPosition(request.PositionType,
                                                 request.Extra,
                                                 context.Deadline.ToTimeout(),
                                                 context.CancellationToken);

            return new GetPositionResponse() { PositionType = res.PositionType, Value = res.Position};
        }

        public override async Task<ResetPositionResponse> ResetPosition(ResetPositionRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            await resource.ResetPosition(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken);
            return new ResetPositionResponse();
        }
    }
}
