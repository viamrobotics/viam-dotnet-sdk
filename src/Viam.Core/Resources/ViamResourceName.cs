using Viam.Common.V1;

namespace Viam.Core.Resources
{
    public record ViamResourceName(string Namespace, string ResourceType, string ResourceSubtype, string Name)
    {
        public ViamResourceName(ResourceName resourceName)
            : this(resourceName.Namespace, resourceName.Type, resourceName.Subtype, resourceName.Name) { }

        public ViamResourceName(SubType subtype, string name)
            : this(new ResourceName()
            {
                Name = name,
                Namespace = subtype.Namespace,
                Type = subtype.ResourceType,
                Subtype = subtype.ResourceSubType
            })
        { }
        // Implicit operator to convert from ViamResource to ResourceName
        public static implicit operator ResourceName(ViamResourceName viamResource) => new ResourceName()
        {
            Name = viamResource.Name,
            Namespace = viamResource.Namespace,
            Type = viamResource.ResourceType,
            Subtype = viamResource.ResourceSubtype
        };

        // Implicit operator to convert from ResourceName to ViamResource
        public static implicit operator ViamResourceName(ResourceName resourceName) => new(resourceName);

        public override string ToString() =>
            $"{Namespace}:{ResourceType}:{ResourceSubtype}:{Name}";
    }
}
