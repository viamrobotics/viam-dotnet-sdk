using Viam.App.V1;
using Viam.Core.Resources;

namespace Viam.ModularResources
{
    public class SimpleModularComponent<T> : IAsyncReconfigurable, IAsyncDisposable
    {
        protected readonly ILogger<T> Logger;

        protected SimpleModularComponent(ILogger<T> logger, ViamResourceName resourceName)
        {
            Logger = logger;
            ResourceName = resourceName;
        }

        public ViamResourceName ResourceName { get; }

        public virtual ValueTask Reconfigure(ComponentConfig config, Dependencies dependencies) => ValueTask.CompletedTask;

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        public virtual ValueTask StopResource() => ValueTask.CompletedTask;

        public virtual ResourceStatus GetStatus() => throw new NotImplementedException();

        public ValueTask<Dictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            if (command.TryGetValue("command", out var cmd))
            {
                Logger.LogDebug("Command: {Command}", cmd);
            }

            return new ValueTask<Dictionary<string, object?>>(new Dictionary<string, object?>());
        }
    }
}