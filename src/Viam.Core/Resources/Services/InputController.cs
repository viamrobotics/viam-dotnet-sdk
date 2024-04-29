using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Component.Inputcontroller.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class InputController(ILogger logger) : InputControllerService.InputControllerServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.inputcontroller.v1.InputControllerService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IInputController)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken).ConfigureAwait(false);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IInputController)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken).ConfigureAwait(false);

            return new GetGeometriesResponse() { Geometries = { res } };
        }

        public override async Task<GetControlsResponse> GetControls(GetControlsRequest request,
                                                                    ServerCallContext context)
        {
            var resource = (IInputController)context.UserState["resource"];
            var res = await resource.GetControls(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new GetControlsResponse() { Controls = { res.Select(x => x.Name) } };
        }

        public override async Task<GetEventsResponse> GetEvents(GetEventsRequest request, ServerCallContext context)
        {
            var resource = (IInputController)context.UserState["resource"];
            var res = await resource.GetEvents(Components.InputController.Control.FromName(request.Controller),
                                               request.Extra,
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken).ConfigureAwait(false);

            return new GetEventsResponse()
            {
                Events =
                       {
                           res.Select(kvp => new Event()
                                             {
                                                 Control = kvp.Key.Name,
                                                 Event_ = kvp.Value.Type.Name,
                                                 Value = kvp.Value.Value,
                                                 Time = Timestamp.FromDateTime(kvp.Value.Timestamp)
                                             })
                       }
            };
        }

        public override Task StreamEvents(StreamEventsRequest request,
                                                IServerStreamWriter<StreamEventsResponse> responseStream,
                                                ServerCallContext context)
        {
            var resource = (IInputController)context.UserState["resource"];
            throw new NotImplementedException();
        }

        public override async Task<TriggerEventResponse> TriggerEvent(TriggerEventRequest request,
                                                                      ServerCallContext context)
        {
            var resource = (IInputController)context.UserState["resource"];
            await resource.TriggerEvent(new Components.InputController.Event(
                                            Components.InputController.Control.FromName(request.Controller),
                                            Components.InputController.EventType.FromName(request.Event.Event_),
                                            request.Event.Time.ToDateTime(),
                                            request.Event.Value),
                                        request.Extra,
                                        context.Deadline.ToTimeout(),
                                        context.CancellationToken).ConfigureAwait(false);

            return new TriggerEventResponse();
        }
    }
}
