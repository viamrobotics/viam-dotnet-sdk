using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Inputcontroller.V1;

namespace Viam.Core.Resources.Services
{
    internal class InputController : InputControllerService.InputControllerServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.inputcontroller.v1.InputControllerService";
        public override Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context) => base.DoCommand(request, context);
        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request, ServerCallContext context) => base.GetGeometries(request, context);
        public override Task<GetControlsResponse> GetControls(GetControlsRequest request, ServerCallContext context) => base.GetControls(request, context);
        public override Task<GetEventsResponse> GetEvents(GetEventsRequest request, ServerCallContext context) => base.GetEvents(request, context);
        public override Task StreamEvents(StreamEventsRequest request, IServerStreamWriter<StreamEventsResponse> responseStream, ServerCallContext context) => base.StreamEvents(request, responseStream, context);
        public override Task<TriggerEventResponse> TriggerEvent(TriggerEventRequest request, ServerCallContext context) => base.TriggerEvent(request, context);
    }
}
