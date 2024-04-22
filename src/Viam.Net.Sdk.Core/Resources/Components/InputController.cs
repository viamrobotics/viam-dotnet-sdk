using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Inputcontroller.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class InputController(ResourceName resourceName, ViamChannel channel) : ComponentBase<InputController, InputControllerService.InputControllerServiceClient>(resourceName, new InputControllerService.InputControllerServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new InputController(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("input_controller");

        public static InputController FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<InputController>(resourceName);
        }

        public override async ValueTask<IDictionary<string, object?>> DoCommandAsync(IDictionary<string, object> command,
            TimeSpan? timeout = null)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
                                                         {
                                                             Name = ResourceName.Name, Command = command.ToStruct()
                                                         });

            return res.Result.ToDictionary();
        }

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

        public ValueTask RegisterControlCallback(Control control,
                                                       Struct? extra = null,
                                                       TimeSpan? timeout = null,
                                                       CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

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

        public abstract record EventType(string name)
        {
            public static EventType FromName(string name)
            {
                return name switch
                       {
                           nameof(AllEvents) => new AllEvents(),
                           nameof(Connect) => new Connect(),
                           nameof(Disconnect) => new Disconnect(),
                           nameof(ButtonPress) => new ButtonPress(),
                           nameof(ButtonRelease) => new ButtonRelease(),
                           nameof(ButtonHold) => new ButtonHold(),
                           nameof(ButtonChange) => new ButtonChange(),
                           nameof(PositionChangeAbs) => new PositionChangeAbs(),
                           nameof(PositionChangeRel) => new PositionChangeRel(),
                           _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown event type")
                       };
            }

            public record AllEvents() : EventType(nameof(AllEvents));
            public record Connect() : EventType(nameof(Connect));
            public record Disconnect() : EventType(nameof(Disconnect));
            public record ButtonPress() : EventType(nameof(ButtonPress));
            public record ButtonRelease() : EventType(nameof(ButtonRelease));
            public record ButtonHold() : EventType(nameof(ButtonHold));
            public record ButtonChange() : EventType(nameof(ButtonChange));
            public record PositionChangeAbs() : EventType(nameof(PositionChangeAbs));
            public record PositionChangeRel() : EventType(nameof(PositionChangeRel));
        }

        public abstract record Control(string name)
        {
            public static Control FromName(string name)
            {
                return name switch
                       {
                           nameof(AbsoluteX)                => new AbsoluteX(),
                           nameof(AbsoluteY)                => new AbsoluteY(),
                           nameof(AbsoluteZ)                => new AbsoluteZ(),
                           nameof(AbsoluteRX)               => new AbsoluteRX(),
                           nameof(AbsoluteRY)               => new AbsoluteRY(),
                           nameof(AbsoluteRZ)               => new AbsoluteRZ(),
                           nameof(AbsoluteHat0X)            => new AbsoluteHat0X(),
                           nameof(AbsoluteHat0Y)            => new AbsoluteHat0Y(),
                           nameof(ButtonSouth)              => new ButtonSouth(),
                           nameof(ButtonEast)               => new ButtonEast(),
                           nameof(ButtonWest)               => new ButtonWest(),
                           nameof(ButtonNorth)              => new ButtonNorth(),
                           nameof(ButtonLT)                 => new ButtonLT(),
                           nameof(ButtonRT)                 => new ButtonRT(),
                           nameof(ButtonLT2)                => new ButtonLT2(),
                           nameof(ButtonRT2)                => new ButtonRT2(),
                           nameof(ButtonLThumb)             => new ButtonLThumb(),
                           nameof(ButtonRThumb)             => new ButtonRThumb(),
                           nameof(ButtonSelect)             => new ButtonSelect(),
                           nameof(ButtonStart)              => new ButtonStart(),
                           nameof(ButtonMenu)               => new ButtonMenu(),
                           nameof(ButtonRecord)             => new ButtonRecord(),
                           nameof(ButtonEStop)              => new ButtonEStop(),
                           nameof(AbsolutePedalAccelerator) => new AbsolutePedalAccelerator(),
                           nameof(AbsolutePedalBrake)       => new AbsolutePedalBrake(),
                           nameof(AbsolutePedalClutch)      => new AbsolutePedalClutch(),
                           _                          => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown control type")
                       };
            }

            public record AbsoluteX() : Control(nameof(AbsoluteX));
            public record AbsoluteY() : Control(nameof(AbsoluteY));
            public record AbsoluteZ() : Control(nameof(AbsoluteZ));
            public record AbsoluteRX() : Control(nameof(AbsoluteRX));
            public record AbsoluteRY() : Control(nameof(AbsoluteRY));
            public record AbsoluteRZ() : Control(nameof(AbsoluteRZ));
            public record AbsoluteHat0X() : Control(nameof(AbsoluteHat0X));
            public record AbsoluteHat0Y() : Control(nameof(AbsoluteHat0Y));
            public record ButtonSouth() : Control(nameof(ButtonSouth));
            public record ButtonEast() : Control(nameof(ButtonEast));
            public record ButtonWest() : Control(nameof(ButtonWest));
            public record ButtonNorth() : Control(nameof(ButtonNorth));
            public record ButtonLT() : Control(nameof(ButtonLT));
            public record ButtonRT() : Control(nameof(ButtonRT));
            public record ButtonLT2() : Control(nameof(ButtonLT2));
            public record ButtonRT2() : Control(nameof(ButtonRT2));
            public record ButtonLThumb() : Control(nameof(ButtonLThumb));
            public record ButtonRThumb() : Control(nameof(ButtonRThumb));
            public record ButtonSelect() : Control(nameof(ButtonSelect));
            public record ButtonStart() : Control(nameof(ButtonStart));
            public record ButtonMenu() : Control(nameof(ButtonMenu));
            public record ButtonRecord() : Control(nameof(ButtonRecord));
            public record ButtonEStop() : Control(nameof(ButtonEStop));
            public record AbsolutePedalAccelerator() : Control(nameof(AbsolutePedalAccelerator));
            public record AbsolutePedalBrake() : Control(nameof(AbsolutePedalBrake));
            public record AbsolutePedalClutch() : Control(nameof(AbsolutePedalClutch));
        }
    }
}
