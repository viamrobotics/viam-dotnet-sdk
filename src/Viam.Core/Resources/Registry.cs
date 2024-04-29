using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Viam.App.V1;
using Viam.Core.Logging;
using Viam.Core.Resources.Components;

namespace Viam.Core.Resources
{
    public static class Registry
    {
        // This is needed to make sure all well-known types get registered at startup
        static Registry()
        {
            Arm.RegisterType();
            Base.RegisterType();
            Board.RegisterType();
            Camera.RegisterType();
            Encoder.RegisterType();
            Gantry.RegisterType();
            Gripper.RegisterType();
            InputController.RegisterType();
            Motor.RegisterType();
            MovementSensor.RegisterType();
            PowerSensor.RegisterType();
            Sensor.RegisterType();
            Servo.RegisterType();
        }
        public static ILogger Logger { get; set; } = NullLogger.Instance;
        private static readonly ConcurrentDictionary<SubType, ResourceRegistration> Subtypes = new(new SubTypeComparer());
        private static readonly ConcurrentDictionary<SubTypeModel, ResourceCreatorRegistration> Resources = new();

        public static bool RegisterSubtype(ResourceRegistration resourceRegistration) => Subtypes.TryAdd(resourceRegistration.SubType, resourceRegistration);

        public static bool RegisterResourceCreator(SubType subType, Model model, ResourceCreatorRegistration resourceRegistration) => Resources.TryAdd(new SubTypeModel(subType, model), resourceRegistration);

        [LogCall]
        public static ResourceRegistration GetResourceRegistrationBySubtype(SubType subType, [CallerMemberName] string? caller = null)
        {
            Logging.LogMessages.LogRegistryResourceGet(Logger, subType);
            if (Subtypes.TryGetValue(subType, out var resourceRegistration))
            {
                Logging.LogMessages.LogRegistryResourceGetSuccess(Logger, subType);
                return resourceRegistration;
            }

            Logging.LogMessages.LogRegistryResourceGetFailure(Logger, subType);
            throw new ResourceRegistrationNotFoundException();
        }

        [LogCall]
        public static ResourceCreatorRegistration GetResourceCreatorRegistration(SubType subType, Model model, [CallerMemberName] string? caller = null)
        {
            Logging.LogMessages.LogRegistryResourceCreatorGet(Logger, subType, model);
            if (Resources.TryGetValue(new SubTypeModel(subType, model), out var resourceCreatorRegistration))
            {
                Logging.LogMessages.LogRegistryResourceCreatorGetSuccess(Logger, subType, model);
                return resourceCreatorRegistration;
            }

            Logging.LogMessages.LogRegistryResourceCreatorGetFailure(Logger, subType, model);
            throw new ResourceCreatorRegistrationNotFoundException();
        }
        // TODO: Implement this
        //public void LookupValidator();

        // TODO: Implement RegisteredSubtypes
        // TODO: Implement RegisteredResourceCreators

        public static ICollection<SubType> RegisteredSubtypes => Subtypes.Keys;
        public static ICollection<SubTypeModel> RegisteredResourceCreators => Resources.Keys;
    }

    public class ResourceRegistration(
        SubType subType,
        Func<ViamResourceName, ViamChannel, ILogger, ResourceBase> clientCreator,
        Func<ILogger, Services.IServiceBase> rpcCreator)
    {
        public SubType SubType => subType;

        internal ResourceBase CreateRpcClient(ViamResourceName name, ViamChannel channel, ILogger logger) => clientCreator(name, channel, logger);
        internal Services.IServiceBase CreateServiceBase(ILogger logger) => rpcCreator(logger);

        public override string ToString() => subType.ToString();
    }

    public record ResourceCreatorRegistration(Func<ComponentConfig, string[], IResourceBase> Creator, Func<ComponentConfig, IEnumerable<string>> ConfigValidator);

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

        public int GetHashCode(SubType obj) => HashCode.Combine(obj.Namespace, obj.ResourceType, obj.ResourceSubType);
    }

    public record struct SubTypeModel(SubType SubType, Model Model)
    {
        public override string ToString() => $"{SubType}/{Model}";
    }

    public record struct Model(ModelFamily Family, string Name)
    {
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
