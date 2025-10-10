using System;
using System.Diagnostics.CodeAnalysis;

using Viam.Common.V1;

namespace Viam.Core.Resources
{
    public record struct ViamResourceName(SubType SubType, string Name)
    {
        public ViamResourceName(ResourceName resourceName)
            : this(new SubType(resourceName.Namespace, resourceName.Type, resourceName.Subtype), resourceName.Name)
        {
        }

        public ResourceName ToResourceName() => new()
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

        public static bool TryParse(string s, [MaybeNullWhen(false)] out ViamResourceName resourceName)
        {
            resourceName = default;
            // If the string is null or empty, throw an exception
            if (string.IsNullOrEmpty(s)) return false;

            // Find separators: ns:type:subtype/name
            var colon1 = s.IndexOf(':');
            if (colon1 <= 0) return false;

            var colon2 = s.IndexOf(':', colon1 + 1);
            if (colon2 <= colon1 + 1) return false;

            var slash = s.IndexOf('/', colon2 + 1);
            if (slash <= colon2 + 1 || slash == s.Length - 1) return false;

            // Extract parts
            var nsPart = s.AsSpan(0, colon1);
            var typePart = s.AsSpan(colon1 + 1, colon2 - (colon1 + 1));
            var subTypePart = s.AsSpan(colon2 + 1, slash - (colon2 + 1));
            var namePart = s.AsSpan(slash + 1);
            var subType = new SubType(nsPart.ToString(), typePart.ToString(), subTypePart.ToString());

            // Set the out parameter
            resourceName = new ViamResourceName(subType, namePart.ToString());

            // Return true to indicate success
            return true;
        }

        public static ViamResourceName Parse(string s)
        {
            // If the string is null or empty, throw an exception
            if (string.IsNullOrEmpty(s)) throw new ArgumentException("Input string is null or empty", nameof(s));

            // Find separators: ns:type:subtype/name
            var colon1 = s.IndexOf(':');
            if (colon1 <= 0) throw new ArgumentException("Input string is not in the correct format", nameof(s));

            var colon2 = s.IndexOf(':', colon1 + 1);
            if (colon2 <= colon1 + 1) throw new ArgumentException("Input string is not in the correct format", nameof(s));

            var slash = s.IndexOf('/', colon2 + 1);
            if (slash <= colon2 + 1 || slash == s.Length - 1) throw new ArgumentException("Input string is not in the correct format", nameof(s));

            // Extract parts
            var nsPart = s.AsSpan(0, colon1);
            var typePart = s.AsSpan(colon1 + 1, colon2 - (colon1 + 1));
            var subTypePart = s.AsSpan(colon2 + 1, slash - (colon2 + 1));
            var namePart = s.AsSpan(slash + 1);
            var subType = new SubType(nsPart.ToString(), typePart.ToString(), subTypePart.ToString());

            // Return the constructed ViamResourceName
            return new ViamResourceName(subType, namePart.ToString());
        }
    }
}