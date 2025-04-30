using Viam.App.V1;
using Viam.Core.Resources;

namespace Viam.ModularResources
{
    public class SimpleModularComponent<T> : IAsyncReconfigurable, IAsyncDisposable
    {
        protected readonly ILogger<T> Logger;

        protected SimpleModularComponent(ILogger<T> logger)
        {
            Logger = logger;
        }

        public ViamResourceName ResourceName { get; }

        public virtual ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies) => ValueTask.CompletedTask;

        public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public virtual ValueTask StopResource() => ValueTask.CompletedTask;

        public virtual ResourceStatus GetStatus() => throw new NotImplementedException();

        public ValueTask<IDictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            //pmic_temp
            if (command.TryGetValue("command", out var cmd))
            {
                Logger.LogDebug("Command: {Command}", cmd);
            }

            return new ValueTask<IDictionary<string, object?>>(new Dictionary<string, object?>());
        }
    }
}
