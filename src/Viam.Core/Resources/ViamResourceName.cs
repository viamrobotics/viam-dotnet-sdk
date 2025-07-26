using Viam.Common.V1;

namespace Viam.Core.Resources
{
    public record ViamResourceName(SubType SubType, string Name)
    {
        public ViamResourceName(ResourceName resourceName)
            : this(new SubType(resourceName.Namespace, resourceName.Type, resourceName.Subtype), resourceName.Name)
        {
        }

        public ResourceName ToResourceName() =>
            new()
            {
                Name = Name,
                Namespace = SubType.Namespace,
                Type = SubType.ResourceType,
                Subtype = SubType.ResourceSubType
            };

        // Implicit operator to convert from ViamResource to ResourceName
        public static implicit operator ResourceName(ViamResourceName viamResource) => new()
        {
            Name = viamResource.Name,
            Namespace = viamResource.SubType.Namespace,
            Type = viamResource.SubType.ResourceType,
            Subtype = viamResource.SubType.ResourceSubType
        };

        // Implicit operator to convert from ResourceName to ViamResource
        public static implicit operator ViamResourceName(ResourceName resourceName) => new(resourceName);

        public override string ToString() =>
            $"{SubType.Namespace}:{SubType.ResourceType}:{SubType.ResourceSubType}/{Name}";
    }
}