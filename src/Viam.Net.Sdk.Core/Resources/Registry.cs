using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Viam.Common.V1;

namespace Viam.Net.Sdk.Core.Resources
{
    public static class Registry
    {
        private static readonly ConcurrentDictionary<SubType, ResourceRegistration> Subtypes = new(new SubTypeComparer());
        private static readonly ConcurrentDictionary<string, ResourceCreatorRegistration> Resources = new();

        public static bool RegisterSubtype(ResourceRegistration resourceRegistration)
        {
            return Subtypes.TryAdd(resourceRegistration.SubType, resourceRegistration);
        }

        public static bool RegisterResourceCreator(SubType subType, Model model, ResourceCreatorRegistration resourceRegistration)
        {
            return Resources.TryAdd($"{subType}/{model}", resourceRegistration);
        }

        public static ResourceRegistration LookupSubtype(SubType subType)
        {
            if (Subtypes.TryGetValue(subType, out var resourceRegistration))
            {
                return resourceRegistration;
            }

            throw new ResourceRegistrationNotFoundException();
        }

        public static ResourceCreatorRegistration ResourceCreatorRegistration(SubType subType, Model model)
        {
            if (Resources.TryGetValue($"{subType}/{model}", out var resourceCreatorRegistration))
            {
                return resourceCreatorRegistration;
            }

            throw new ResourceCreatorRegistrationNotFoundException();
        }

        // TODO: Implement this
        //public void LookupValidator();

        // TODO: Implement RegisteredSubtypes
        // TODO: Implement RegisteredResourceCreators
    }

    public class ResourceRegistration(
        SubType subType,
        Func<ResourceName, ViamChannel, ResourceBase> clientCreator,
        Func<object> statusCreator)
    {
        public SubType SubType => subType;
        private readonly Func<ResourceName, ViamChannel, ResourceBase> _clientCreator = clientCreator;
        private Func<object> _statusCreator = statusCreator;

        internal ResourceBase CreateRpcClient(ResourceName name, ViamChannel channel) => _clientCreator(name, channel);
    }

    public record class ResourceCreatorRegistration();

    public record class ResourceCreator();

    public record class SubType(string Namespace, string ResourceType, string ResourceSubType)
    {
        public override string ToString() => $"{Namespace}:{ResourceType}:{ResourceSubType}";
        public static SubType FromRdkComponent(string componentType) => new("rdk", "component", componentType);
        public static SubType FromRdkService(string serviceType) => new("rdk", "service", serviceType);
        public static SubType Default = new("none", "none", "none");

        public static SubType FromResourceName(ResourceName resourceName) =>
            new SubType(resourceName.Namespace, resourceName.Type, resourceName.Subtype);
    }

    public record class SubTypeComparer : IEqualityComparer<SubType>
    {
        public bool Equals(SubType x, SubType y)
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
            return HashCode.Combine(obj.Namespace, obj.ResourceType, obj.ResourceSubType);
        }
    }

    public class ResourceRegistrationNotFoundException : Exception;

    public class ResourceCreatorRegistrationNotFoundException : Exception;

    public record class Model(ModelFamily Family, string Name);

    public record class ModelFamily(string Namespace, string Family);
}
