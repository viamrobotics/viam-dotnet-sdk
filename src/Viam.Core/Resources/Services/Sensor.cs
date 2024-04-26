using System;
using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Sensor.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Sensor(ResourceManager resourceManager) : SensorService.SensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.sensor.v1.SensorService";

        private ISensor GetResource(string name)
        {
            if (resourceManager.GetResourceByShortName(name) is not ISensor res)
            {
                throw new ResourceNotFoundException(name);
            }
            return res;
        }

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = GetResource(request.Name);
            var res = await resource.DoCommand(request.Command.ToDictionary(), context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context)
        {
            var resource = GetResource(request.Name);
            var res = await resource.GetReadings(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);

            var r = new GetReadingsResponse();
            r.Readings.AddRange(res);
            return r;
        }
    }
}
