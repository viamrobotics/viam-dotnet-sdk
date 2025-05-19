using System;
using System.Collections.Generic;
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
    public record struct Service(string Name)
    {
        public static Service ArmService => new("viam.component.arm.v1.ArmService");
        public static Service BaseService => new("viam.component.base.v1.BaseService");
        public static Service BoardService => new("viam.component.board.v1.BoardService");
        public static Service CameraService => new("viam.component.camera.v1.CameraService");
        public static Service EncoderService => new("viam.component.encoder.v1.EncoderService");
        public static Service GantryService => new("viam.component.gantry.v1.GantryService");
        public static Service GenericService => new("viam.component.generic.v1.GenericService");
        public static Service GripperService => new("viam.component.gripper.v1.GripperService");
        public static Service InputControllerService => new("viam.component.inputcontroller.v1.InputControllerService");
        public static Service MotorService => new("viam.component.motor.v1.MotorService");
        public static Service MovementSensorService => new("viam.component.movementsensor.v1.MovementSensorService");
        public static Service PowerSensorService => new("viam.component.powersensor.v1.PowerSensorService");
        public static Service SensorService => new("viam.component.sensor.v1.SensorService");
        public static Service ServoService => new("viam.component.servo.v1.ServoService");
        public static implicit operator string(Service serviceName) => serviceName.Name;

        private static IDictionary<Type, Service> _serviceMap = new Dictionary<Type, Service>()
        {
            { typeof(IArm), ArmService },
            { typeof(IBase), BaseService },
            { typeof(IBoard), BoardService },
            { typeof(ICamera), CameraService },
            { typeof(IEncoder), EncoderService },
            { typeof(IGantry), GantryService },
            { typeof(IGeneric), GenericService },
            { typeof(IGripper), GripperService },
            { typeof(IInputController), InputControllerService },
            { typeof(IMotor), MotorService },
            { typeof(IMovementSensor), MovementSensorService },
            { typeof(IPowerSensor), PowerSensorService },
            { typeof(ISensor), SensorService },
            { typeof(IServo), ServoService }
        };

        public static Service Lookup<T>() where T : IResourceBase
        {
            if (_serviceMap.TryGetValue(typeof(T), out var service))
                return service;
            throw new InvalidOperationException($"Unknown service type {typeof(T)}");
        }

        public static void RegisterCustomService<T>(string serviceName) where T : IResourceBase
        {
            _serviceMap[typeof(T)] = new Service(serviceName);
        }
    }
}