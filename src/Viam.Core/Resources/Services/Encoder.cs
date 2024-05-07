using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Encoder(ILogger<Encoder> logger) : EncoderService.EncoderServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.encoder.v1.EncoderService";
        public SubType SubType { get; } = SubType.FromRdkComponent("encoder");

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken).ConfigureAwait(false);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken).ConfigureAwait(false);

            return new GetGeometriesResponse() { Geometries = { res } };
        }

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.GetProperties(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken).ConfigureAwait(false);

            return new GetPropertiesResponse()
            {
                AngleDegreesSupported = res.AngleDegreesSupported,
                TicksCountSupported = res.TicksCountSupported
            };
        }

        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request,
                                                                    ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            var res = await resource.GetPosition(request.PositionType,
                                                 request.Extra,
                                                 context.Deadline.ToTimeout(),
                                                 context.CancellationToken).ConfigureAwait(false);

            return new GetPositionResponse() { PositionType = res.PositionType, Value = res.Position };
        }

        public override async Task<ResetPositionResponse> ResetPosition(ResetPositionRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IEncoder)context.UserState["resource"];
            await resource.ResetPosition(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new ResetPositionResponse();
        }
    }
}
