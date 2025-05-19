using System;
using System.Collections.Generic;

namespace Viam.Core.Resources
{
    public abstract class ResourceStatus(ViamResourceName resourceName)
    {
        public readonly ViamResourceName ResourceName = resourceName;
        public abstract DateTime? LastReconfigured { get; protected set; }
        public abstract IDictionary<string, object?> Details { get; }

        public static Func<ResourceBase, ResourceStatus> DefaultCreator => @base =>
            new DefaultResourceStatus(@base.ResourceName, @base.LastReconfigured);
    }

    internal sealed class DefaultResourceStatus(ViamResourceName resourceName, DateTime? lastReconfigured)
        : ResourceStatus(resourceName)
    {
        public override DateTime? LastReconfigured { get; protected set; } = lastReconfigured;
        public override IDictionary<string, object?> Details { get; } = new Dictionary<string, object?>();
    }
}