using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using Viam.App.V1;
using Viam.Core.Logging;
using Viam.Core.Resources.Components;
using Viam.Core.Resources.Components.Arm;
using Viam.Core.Resources.Components.Base;
using Viam.Core.Resources.Components.Board;
using Viam.Core.Resources.Components.Camera;
using Viam.Core.Resources.Components.Encoder;
using Viam.Core.Resources.Components.Gantry;
using Viam.Core.Resources.Components.Generic;
using Viam.Core.Resources.Components.Gripper;
using Viam.Core.Resources.Components.InputController;
using Viam.Core.Resources.Components.Motor;
using Viam.Core.Resources.Components.MovementSensor;
using Viam.Core.Resources.Components.PowerSensor;
using Viam.Core.Resources.Components.Sensor;
using Viam.Core.Resources.Components.Servo;

namespace Viam.Core.Resources
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "C# is dumb sometimes")]
    public record struct SubType(string Namespace, string ResourceType, string ResourceSubType)
    {
        public override string ToString() => $"{Namespace}:{ResourceType}:{ResourceSubType}";
        public static SubType FromRdkComponent(string componentType) => new("rdk", "component", componentType);
        public static SubType FromRdkService(string serviceType) => new("rdk", "service", serviceType);
        public static SubType Default = new("none", "none", "none");
        public static SubType Arm = FromRdkComponent("arm");
        public static SubType Base = FromRdkComponent("base");
        public static SubType Board = FromRdkComponent("board");
        public static SubType Camera = FromRdkComponent("camera");
        public static SubType Encoder = FromRdkComponent("encoder");
        public static SubType Gantry = FromRdkComponent("gantry");
        public static SubType Generic = FromRdkComponent("generic");
        public static SubType Gripper = FromRdkComponent("gripper");
        public static SubType InputController = FromRdkComponent("input_controller");
        public static SubType Motor = FromRdkComponent("motor");
        public static SubType MovementSensor = FromRdkComponent("movement_sensor");
        public static SubType PowerSensor = FromRdkComponent("power_sensor");
        public static SubType Sensor = FromRdkComponent("sensor");
        public static SubType Servo = FromRdkComponent("servo");

        public static SubType FromResourceName(ViamResourceName resourceName) =>
            new(resourceName.SubType.Namespace, resourceName.SubType.ResourceType, resourceName.SubType.ResourceSubType);

        public static SubType FromString(string str)
        {
            var parts = str.Split(':');
            if (parts.Length != 3)
            {
                throw new ArgumentException($"{str} is not a valid SubType");
            }
            var @namespace = parts[0];
            var resourceType = parts[1];
            var resourceSubType = parts[2];
            return new SubType(@namespace, resourceType, resourceSubType);
        }
    }

    public record struct SubTypeModel(SubType SubType, Model Model)
    {
        public override string ToString() => $"{SubType}/{Model}";
    }

    public record struct Model(ModelFamily Family, string Name)
    {
        public Model(string @namespace, string family, string name) : this(new ModelFamily(@namespace, family), name) { }
        public override string ToString() => $"{Family}:{Name}";

        public static Model FromString(string str)
        {
            var parts = str.Split(':');
            if (parts.Length != 3)
            {
                throw new ArgumentException($"{str} is not a valid Model");
            }
            return new Model(new ModelFamily(parts[0], parts[1]), parts[2]);
        }
    }

    public record struct ModelFamily(string Namespace, string Family)
    {
        public override string ToString() => $"{Namespace}:{Family}";
    }
}
