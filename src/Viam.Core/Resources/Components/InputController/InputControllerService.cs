using System;
using System.Linq;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Inputcontroller.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.InputController
{
    internal class InputControllerService(ILogger<InputControllerService> logger) : Component.Inputcontroller.V1.InputControllerService.InputControllerServiceBase, IServiceBase
    {
        public static Service ServiceName => Service.InputControllerService;
        public static SubType SubType { get; } = SubType.InputController;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IInputController)context.UserState["resource"];
                var res = await resource.DoCommand(request.Command.ToDictionary(),
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken).ConfigureAwait(false);

                var response = new DoCommandResponse() { Result = res.ToStruct() };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IInputController)context.UserState["resource"];
                var res = await resource.GetGeometries(request.Extra?.ToDictionary(),
                                                       context.Deadline.ToTimeout(),
                                                       context.CancellationToken).ConfigureAwait(false);

                var response = new GetGeometriesResponse() { Geometries = { res } };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetControlsResponse> GetControls(GetControlsRequest request,
                                                                    ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IInputController)context.UserState["resource"];
                var res = await resource.GetControls(request.Extra?.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
                var response = new GetControlsResponse() { Controls = { res.Select(x => x.Name) } };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetEventsResponse> GetEvents(GetEventsRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IInputController)context.UserState["resource"];
                var res = await resource.GetEvents(InputControllerClient.Control.FromName(request.Controller),
                                                   request.Extra?.ToDictionary(),
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken).ConfigureAwait(false);

                var response = new GetEventsResponse()
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
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override Task StreamEvents(StreamEventsRequest request,
                                                IServerStreamWriter<StreamEventsResponse> responseStream,
                                                ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IInputController)context.UserState["resource"];
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<TriggerEventResponse> TriggerEvent(TriggerEventRequest request,
                                                                      ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IInputController)context.UserState["resource"];
                await resource.TriggerEvent(new InputControllerClient.Event(
                                                InputControllerClient.Control.FromName(request.Controller),
                                                InputControllerClient.EventType.FromName(request.Event.Event_),
                                                request.Event.Time.ToDateTime(),
                                                request.Event.Value),
                                            request.Extra?.ToDictionary(),
                                            context.Deadline.ToTimeout(),
                                            context.CancellationToken).ConfigureAwait(false);

                var response = new TriggerEventResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}
