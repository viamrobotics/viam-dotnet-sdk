using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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
    public static class Registry
    {
        public static ILogger Logger { get; set; } = NullLogger.Instance;
        private static readonly ConcurrentDictionary<SubType, ComponentRegistration> Subtypes = new(new SubTypeComparer());
        private static readonly ConcurrentDictionary<SubTypeModel, ResourceCreatorRegistration> Resources = new();

        private static readonly IDictionary<SubType, ComponentRegistration> BuiltIn =
            new Dictionary<SubType, ComponentRegistration>()
            {
                { BaseClient.SubType, new ComponentRegistration(BaseClient.SubType, (name, channel, logger) => new BaseClient(name, channel, logger)) },
                { BoardClient.SubType, new ComponentRegistration(BoardClient.SubType, (name, channel, logger) => new BoardClient(name, channel, logger)) },
                { CameraClient.SubType, new ComponentRegistration(CameraClient.SubType, (name, channel, logger) => new CameraClient(name, channel, logger)) },
                { EncoderClient.SubType, new ComponentRegistration(EncoderClient.SubType, (name, channel, logger) => new EncoderClient(name, channel, logger)) },
                { GantryClient.SubType, new ComponentRegistration(GantryClient.SubType, (name, channel, logger) => new GantryClient(name, channel, logger)) },
                { GripperClient.SubType, new ComponentRegistration(GripperClient.SubType, (name, channel, logger) => new GripperClient(name, channel, logger)) },
                { InputControllerClient.SubType, new ComponentRegistration(InputControllerClient.SubType, (name, channel, logger) => new InputControllerClient(name, channel, logger)) },
                { MotorClient.SubType, new ComponentRegistration(MotorClient.SubType, (name, channel, logger) => new MotorClient(name, channel, logger)) },
                { MovementSensorClient.SubType, new ComponentRegistration(MovementSensorClient.SubType, (name, channel, logger) => new MovementSensorClient(name, channel, logger)) },
                { PowerSensorClient.SubType, new ComponentRegistration(PowerSensorClient.SubType, (name, channel, logger) => new PowerSensorClient(name, channel, logger)) },
                { SensorClient.SubType, new ComponentRegistration(SensorClient.SubType, (name, channel, logger) => new SensorClient(name, channel, logger)) },
                { ServoClient.SubType, new ComponentRegistration(ServoClient.SubType, (name, channel, logger) => new ServoClient(name, channel, logger)) },
            };

        public static bool RegisterSubtype(ComponentRegistration componentRegistration) => Subtypes.TryAdd(componentRegistration.SubType, componentRegistration);

        public static bool RegisterResourceCreator(SubType subType, Model model, ResourceCreatorRegistration resourceRegistration) => Resources.TryAdd(new SubTypeModel(subType, model), resourceRegistration);

        /// <summary>
        /// Register the default services to the <see cref="IServiceCollection"/> if a component of that type was registered
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> for the ASP.NET server</param>
        public static void RegisterComponentServices(IServiceCollection services)
        {
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("arm")))
                RegisterService<ArmService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("base")))
                RegisterService<BaseService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("board")))
                RegisterService<BoardService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("camera")))
                RegisterService<CameraService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("encoder")))
                RegisterService<EncoderService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("gantry")))
                RegisterService<GantryService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("generic")))
                RegisterService<GenericService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("gripper")))
                RegisterService<GripperService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("input_controller")))
                RegisterService<InputControllerService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("motor")))
                RegisterService<MotorService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("movement_sensor")))
                RegisterService<MovementSensorService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("power_sensor")))
                RegisterService<PowerSensorService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("sensor")))
                RegisterService<SensorService>(services);
            if (Subtypes.ContainsKey(SubType.FromRdkComponent("servo")))
                RegisterService<ServoService>(services);
        }

        private static void RegisterService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImpl>(IServiceCollection services) where TImpl : class, IServiceBase
        {
            services.AddTransient<TImpl>();
            services.AddTransient<IServiceBase, TImpl>();
        }
        
        public static ComponentRegistration GetResourceRegistrationBySubtype(SubType subType, [CallerMemberName] string? caller = null)
        {
            Logger.LogRegistryResourceGet(subType);

            if (BuiltIn.TryGetValue(subType, out var builtInResourceRegistration))
            {
                Logger.LogRegistryBuiltInResourceGetSuccess(subType);
                return builtInResourceRegistration;
            }

            if (Subtypes.TryGetValue(subType, out var resourceRegistration))
            {
                Logger.LogRegistryResourceGetSuccess(subType);
                return resourceRegistration;
            }

            Logger.LogRegistryResourceGetFailure(subType);
            throw new ResourceRegistrationNotFoundException();
        }

        
        public static ResourceCreatorRegistration GetResourceCreatorRegistration(SubType subType, Model model, [CallerMemberName] string? caller = null)
        {
            Logger.LogRegistryResourceCreatorGet(subType, model);
            if (Resources.TryGetValue(new SubTypeModel(subType, model), out var resourceCreatorRegistration))
            {
                Logger.LogRegistryResourceCreatorGetSuccess(subType, model);
                return resourceCreatorRegistration;
            }

            Logger.LogRegistryResourceCreatorGetFailure(subType, model);
            throw new ResourceCreatorRegistrationNotFoundException();
        }
        // TODO: Implement this
        //public void LookupValidator();

        // TODO: Implement RegisteredSubtypes
        // TODO: Implement RegisteredResourceCreators

        public static ICollection<SubType> RegisteredSubtypes => Subtypes.Keys;
        public static ICollection<SubTypeModel> RegisteredResourceCreators => Resources.Keys;
    }

    public class ComponentRegistration(SubType subType, Func<ViamResourceName, ViamChannel, ILogger, ResourceBase> clientCreator)
    {
        public SubType SubType => subType;

        
        internal ResourceBase CreateRpcClient(ViamResourceName name, ViamChannel channel, ILogger logger) => clientCreator(name, channel, logger);

        public override string ToString() => subType.ToString();
    }

    public record ResourceCreatorRegistration(Func<ILogger, ComponentConfig, IDictionary<ViamResourceName, IResourceBase>, IResourceBase> Creator, Func<ComponentConfig, IEnumerable<string>> ConfigValidator);

    public record SubType(string Namespace, string ResourceType, string ResourceSubType)
    {
        public override string ToString() => $"{Namespace}:{ResourceType}:{ResourceSubType}";
        public static SubType FromRdkComponent(string componentType) => new("rdk", "component", componentType);
        public static SubType FromRdkService(string serviceType) => new("rdk", "service", serviceType);
        public static SubType Default = new("none", "none", "none");

        public static SubType FromResourceName(ViamResourceName resourceName) =>
            new(resourceName.Namespace, resourceName.ResourceType, resourceName.ResourceSubtype);

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

    public record SubTypeComparer : IEqualityComparer<SubType>
    {
        public bool Equals(SubType? x, SubType? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

            return x.Namespace == y.Namespace
                && x.ResourceType == y.ResourceType
                && x.ResourceSubType == y.ResourceSubType;
        }

        public int GetHashCode(SubType obj)
        {
            unchecked
            {
                var hashCode = obj.Namespace.GetHashCode();
                hashCode = (hashCode * 397) ^ (obj.ResourceType.GetHashCode());
                hashCode = (hashCode * 397) ^ (obj.ResourceSubType.GetHashCode());
                return hashCode;
            }
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
