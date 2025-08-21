using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Viam.Core.Resources
{
    public abstract class ResourceBase(ViamResourceName resourceName) : IResourceBase
    {
        private bool _disposed;
        private int _disposedGuard;
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
            if (Interlocked.CompareExchange(ref _disposedGuard, 1, 0) == 1)
                return ValueTask.CompletedTask;
            _disposed = true;
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        private protected void ThrowIfDisposed()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
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