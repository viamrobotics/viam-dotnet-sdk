using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viam.Common.V1;

namespace Viam.Core.Resources
{
    public interface IResourceBase
    {
        public ResourceName ResourceName { get; }
        public static ResourceName GetResourceName(SubType subtype, string name) =>
            new()
            {
                Name = name,
                Namespace = subtype.Namespace,
                Type = subtype.ResourceType,
                Subtype = subtype.ResourceSubType
            };

        public ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
                                                                 TimeSpan? timeout = null,
                                                                 CancellationToken cancellationToken = default);
        public ValueTask StopResource();
        public ResourceStatus GetStatus();
    }
    public abstract class ResourceBase(ResourceName resourceName) : IResourceBase
    {
        public ResourceName ResourceName => resourceName;
        public abstract DateTime? LastReconfigured { get; }
        public virtual ResourceStatus GetStatus() => ResourceStatus.DefaultCreator(this);
        public abstract ValueTask StopResource();
        public string Name => ResourceName.Name;
        public abstract ValueTask<IDictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }
}
