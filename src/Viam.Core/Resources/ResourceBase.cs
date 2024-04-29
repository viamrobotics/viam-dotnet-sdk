using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Viam.Core.Resources
{
    public abstract class ResourceBase(ViamResourceName resourceName) : IResourceBase
    {
        public ViamResourceName ResourceName => resourceName;
        public abstract DateTime? LastReconfigured { get; }
        public virtual ResourceStatus GetStatus() => ResourceStatus.DefaultCreator(this);
        public abstract ValueTask StopResource();
        public string Name => ResourceName.Name;
        public abstract ValueTask<IDictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
        public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    public interface IResourceBase : IAsyncDisposable
    {
        public ViamResourceName ResourceName { get; }

        public ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
                                                                 TimeSpan? timeout = null,
                                                                 CancellationToken cancellationToken = default);
        public ValueTask StopResource();
        public ResourceStatus GetStatus();
    }
}
