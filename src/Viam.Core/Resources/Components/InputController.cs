using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Inputcontroller.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IInputController : IComponentBase
    {
        ValueTask<InputController.Control[]> GetControls(Struct? extra = null,
                                         TimeSpan? timeout = null,
                                         CancellationToken cancellationToken = default);

        ValueTask<IDictionary<InputController.Control, InputController.Event>> GetEvents(InputController.Control control,
                                         Struct? extra = null,
                                         TimeSpan? timeout = null,
                                         CancellationToken cancellationToken = default);

        ValueTask RegisterControlCallback(InputController.Control control,
                                          Struct? extra = null,
                                          TimeSpan? timeout = null,
                                          CancellationToken cancellationToken = default);

        ValueTask TriggerEvent(InputController.Event @event,
                               Struct? extra = null,
                               TimeSpan? timeout = null,
                               CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }
    public class InputController(ViamResourceName resourceName, ViamChannel channel, ILogger logger) : 
        ComponentBase<InputController, InputControllerService.InputControllerServiceClient>(resourceName, new InputControllerService.InputControllerServiceClient(channel)), 
        IInputController
    {
        internal static void RegisterType() => Registry.RegisterSubtype(
            new ResourceRegistration(SubType,
                                                      (name, channel, logger) => new InputController(name, channel, logger),
                                                      (logger) => new Services.InputController(logger)));
        public static SubType SubType = SubType.FromRdkComponent("input_controller");

        [LogCall]
        public static InputController FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<InputController>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        [LogCall]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .DoCommandAsync(
                                new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                                deadline: timeout.ToDeadline(),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogCall]
        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(
                                      new GetGeometriesRequest() { Name = ResourceName.Name, Extra = extra },
                                      deadline: timeout.ToDeadline(),
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Geometries.ToArray();
        }

        [LogCall]
        public async ValueTask<Control[]> GetControls(Struct? extra = null,
                                                      TimeSpan? timeout = null,
                                                      CancellationToken cancellationToken = default)
        {
            var res = await Client.GetControlsAsync(new GetControlsRequest() { Controller = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Controls.Select(Control.FromName).ToArray();
        }

        [LogCall]
        public async ValueTask<IDictionary<Control, Event>> GetEvents(Control control,
                                         Struct? extra = null,
                                         TimeSpan? timeout = null,
                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetEventsAsync(new GetEventsRequest() { Controller = Name, Extra = extra },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Events.ToDictionary(x => Control.FromName(x.Control), Event.FromProto);
        }

        [LogCall]
        public ValueTask RegisterControlCallback(Control control,
                                                       Struct? extra = null,
                                                       TimeSpan? timeout = null,
                                                       CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        [LogCall]
        public ValueTask TriggerEvent(Event @event,
                                      Struct? extra = null,
                                      TimeSpan? timeout = null,
                                      CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

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
