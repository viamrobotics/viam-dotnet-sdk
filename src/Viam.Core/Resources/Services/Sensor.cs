using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Sensor.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Sensor(ILogger logger) : SensorService.SensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.sensor.v1.SensorService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (ISensor)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context)
        {
            var resource = (ISensor)context.UserState["resource"];
            var res = await resource.GetReadings(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);

            var r = new GetReadingsResponse();
            r.Readings.Add(res);
            return r;
        }
    }
}
