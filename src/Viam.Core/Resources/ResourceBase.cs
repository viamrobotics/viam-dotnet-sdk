using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Viam.Core.Resources
{
    public abstract class ResourceBase(ViamResourceName resourceName) : IResourceBase
    {
        public ViamResourceName ResourceName { get; } = resourceName;
        public string Name => ResourceName.Name;
        public abstract DateTime? LastReconfigured { get; }
        public virtual ResourceStatus GetStatus() => ResourceStatus.DefaultCreator(this);
        public abstract ValueTask StopResource();

        public abstract ValueTask<Dictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }

    public interface IResourceBase : IAsyncDisposable
    {
        public ViamResourceName ResourceName { get; }

        public ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public ValueTask StopResource();
        public ResourceStatus GetStatus();
    }
}