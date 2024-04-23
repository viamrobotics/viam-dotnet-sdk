using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Viam.App.V1;
using Viam.Common.V1;

namespace Viam.Core.Resources
{
    public static class Registry
    {
        private static readonly ConcurrentDictionary<SubType, ResourceRegistration> Subtypes = new(new SubTypeComparer());
        private static readonly ConcurrentDictionary<string, ResourceCreatorRegistration> Resources = new();

        public static bool RegisterSubtype(ResourceRegistration resourceRegistration) => Subtypes.TryAdd(resourceRegistration.SubType, resourceRegistration);

        public static bool RegisterResourceCreator(SubType subType, Model model, ResourceCreatorRegistration resourceRegistration) => Resources.TryAdd($"{subType}/{model}", resourceRegistration);

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
        Func<ResourceName, ViamChannel, ResourceBase> clientCreator)
    {
        public SubType SubType => subType;

        internal ResourceBase CreateRpcClient(ResourceName name, ViamChannel channel) => clientCreator(name, channel);
    }

    public record ResourceCreatorRegistration(Func<ComponentConfig, Dictionary<ResourceName, ResourceBase>, IResourceBase> creator, Func<ComponentConfig, IEnumerable<string>> validator);

    public record SubType(string Namespace, string ResourceType, string ResourceSubType)
    {
        public override string ToString() => $"{Namespace}:{ResourceType}:{ResourceSubType}";
        public static SubType FromRdkComponent(string componentType) => new("rdk", "component", componentType);
        public static SubType FromRdkService(string serviceType) => new("rdk", "service", serviceType);
        public static SubType Default = new("none", "none", "none");

        public static SubType FromResourceName(ResourceName resourceName) =>
            new SubType(resourceName.Namespace, resourceName.Type, resourceName.Subtype);
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

    public class ResourceRegistrationNotFoundException : Exception;

    public class ResourceCreatorRegistrationNotFoundException : Exception;

    public record Model(ModelFamily Family, string Name);

    public record ModelFamily(string Namespace, string Family);
}
