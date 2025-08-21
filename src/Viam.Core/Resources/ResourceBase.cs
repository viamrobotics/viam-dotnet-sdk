using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Viam.Core.Resources
{
    public abstract class ResourceBase(ViamResourceName resourceName, ILogger logger) : IResourceBase
    {
        private bool _disposed;
        private int _disposedGuard;
        private protected ILogger Logger = logger;
        public ViamResourceName ResourceName { get; } = resourceName;

        public abstract ValueTask StopResource();

        public abstract DateTime? LastReconfigured { get; }

        public abstract ValueTask<Dictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        public virtual ResourceStatus GetStatus() => ResourceStatus.DefaultCreator(this);

        
        public string Name => ResourceName.Name;
        public virtual ValueTask DisposeAsync()
        {
            Logger.LogTrace("Resource is disposing");
            if (Interlocked.CompareExchange(ref _disposedGuard, 1, 0) == 1)
                return ValueTask.CompletedTask;
            _disposed = true;
            GC.SuppressFinalize(this);
            Logger.LogTrace("Resource is disposed");
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