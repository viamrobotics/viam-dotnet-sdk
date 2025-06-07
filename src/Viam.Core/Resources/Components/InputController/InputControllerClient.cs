using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Inputcontroller.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.InputController
{
    public class InputControllerClient(ViamResourceName resourceName, ViamChannel channel, ILogger<InputControllerClient> logger)
        : ComponentBase<InputControllerClient,
                Component.Inputcontroller.V1.InputControllerService.InputControllerServiceClient>(
                resourceName,
                new Component.Inputcontroller.V1.InputControllerService.InputControllerServiceClient(channel)),
            IInputController
    {
        public static SubType SubType = SubType.FromRdkComponent("input_controller");


        public static IInputController FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<InputControllerClient>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => new ValueTask();

        public override async ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                    .DoCommandAsync(
                        new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result.ToDictionary();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = ResourceName.Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var geometry = res.Geometries.ToArray();
                logger.LogMethodInvocationSuccess(results: geometry);
                return geometry;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Control[]> GetControls(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetControlsAsync(
                        new GetControlsRequest() { Controller = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Controls.Select(Control.FromName)
                    .ToArray();

                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Dictionary<Control, Event>> GetEvents(Control control,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetEventsAsync(
                        new GetEventsRequest() { Controller = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Events.ToDictionary(x => Control.FromName(x.Control), Event.FromProto);
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public ValueTask RegisterControlCallback(Control control,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                throw new NotImplementedException();
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public ValueTask TriggerEvent(Event @event,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                throw new NotImplementedException();
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public record Event(Control Control, EventType Type, DateTime Timestamp, double Value)
        {
            public static Event FromProto(Component.Inputcontroller.V1.Event @event) =>
                new(Control.FromName(@event.Control),
                    EventType.FromName(@event.Event_),
                    @event.Time.ToDateTime(),
                    @event.Value);
        }

        public record EventType(string Name)
        {
            public static EventType FromName(string name)
            {
                return name switch
                {
                    nameof(AllEvents) => AllEvents,
                    nameof(Connect) => Connect,
                    nameof(Disconnect) => Disconnect,
                    nameof(ButtonPress) => ButtonPress,
                    nameof(ButtonRelease) => ButtonRelease,
                    nameof(ButtonHold) => ButtonHold,
                    nameof(ButtonChange) => ButtonChange,
                    nameof(PositionChangeAbs) => PositionChangeAbs,
                    nameof(PositionChangeRel) => PositionChangeRel,
                    _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown event type")
                };
            }

            public static EventType AllEvents = new(nameof(AllEvents));
            public static EventType Connect = new(nameof(Connect));
            public static EventType Disconnect = new(nameof(Disconnect));
            public static EventType ButtonPress = new(nameof(ButtonPress));
            public static EventType ButtonRelease = new(nameof(ButtonRelease));
            public static EventType ButtonHold = new(nameof(ButtonHold));
            public static EventType ButtonChange = new(nameof(ButtonChange));
            public static EventType PositionChangeAbs = new(nameof(PositionChangeAbs));
            public static EventType PositionChangeRel = new(nameof(PositionChangeRel));
        }

        public record Control(string Name)
        {
            public static Control FromName(string name)
            {
                return name switch
                {
                    nameof(AbsoluteX) => AbsoluteX,
                    nameof(AbsoluteY) => AbsoluteY,
                    nameof(AbsoluteZ) => AbsoluteZ,
                    nameof(AbsoluteRX) => AbsoluteRX,
                    nameof(AbsoluteRY) => AbsoluteRY,
                    nameof(AbsoluteRZ) => AbsoluteRZ,
                    nameof(AbsoluteHat0X) => AbsoluteHat0X,
                    nameof(AbsoluteHat0Y) => AbsoluteHat0Y,
                    nameof(ButtonSouth) => ButtonSouth,
                    nameof(ButtonEast) => ButtonEast,
                    nameof(ButtonWest) => ButtonWest,
                    nameof(ButtonNorth) => ButtonNorth,
                    nameof(ButtonLT) => ButtonLT,
                    nameof(ButtonRT) => ButtonRT,
                    nameof(ButtonLT2) => ButtonLT2,
                    nameof(ButtonRT2) => ButtonRT2,
                    nameof(ButtonLThumb) => ButtonLThumb,
                    nameof(ButtonRThumb) => ButtonRThumb,
                    nameof(ButtonSelect) => ButtonSelect,
                    nameof(ButtonStart) => ButtonStart,
                    nameof(ButtonMenu) => ButtonMenu,
                    nameof(ButtonRecord) => ButtonRecord,
                    nameof(ButtonEStop) => ButtonEStop,
                    nameof(AbsolutePedalAccelerator) => AbsolutePedalAccelerator,
                    nameof(AbsolutePedalBrake) => AbsolutePedalBrake,
                    nameof(AbsolutePedalClutch) => AbsolutePedalClutch,
                    _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown control type")
                };
            }

            public static Control AbsoluteX = new(nameof(AbsoluteX));
            public static Control AbsoluteY = new(nameof(AbsoluteY));
            public static Control AbsoluteZ = new(nameof(AbsoluteZ));
            public static Control AbsoluteRX = new(nameof(AbsoluteRX));
            public static Control AbsoluteRY = new(nameof(AbsoluteRY));
            public static Control AbsoluteRZ = new(nameof(AbsoluteRZ));
            public static Control AbsoluteHat0X = new(nameof(AbsoluteHat0X));
            public static Control AbsoluteHat0Y = new(nameof(AbsoluteHat0Y));
            public static Control ButtonSouth = new(nameof(ButtonSouth));
            public static Control ButtonEast = new(nameof(ButtonEast));
            public static Control ButtonWest = new(nameof(ButtonWest));
            public static Control ButtonNorth = new(nameof(ButtonNorth));
            public static Control ButtonLT = new(nameof(ButtonLT));
            public static Control ButtonRT = new(nameof(ButtonRT));
            public static Control ButtonLT2 = new(nameof(ButtonLT2));
            public static Control ButtonRT2 = new(nameof(ButtonRT2));
            public static Control ButtonLThumb = new(nameof(ButtonLThumb));
            public static Control ButtonRThumb = new(nameof(ButtonRThumb));
            public static Control ButtonSelect = new(nameof(ButtonSelect));
            public static Control ButtonStart = new(nameof(ButtonStart));
            public static Control ButtonMenu = new(nameof(ButtonMenu));
            public static Control ButtonRecord = new(nameof(ButtonRecord));
            public static Control ButtonEStop = new(nameof(ButtonEStop));
            public static Control AbsolutePedalAccelerator = new(nameof(AbsolutePedalAccelerator));
            public static Control AbsolutePedalBrake = new(nameof(AbsolutePedalBrake));
            public static Control AbsolutePedalClutch = new(nameof(AbsolutePedalClutch));
        }
    }
}