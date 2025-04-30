using Viam.App.V1;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;

namespace Viam.ModularResources
{
    public interface IModularResource : IResourceBase, IDisposable
    {
        public string Name => ResourceName.Name;
        public string[] ValidateConfig(App.V1.ComponentConfig config);
    }

    public interface IModularResourceService : IModularResource, IServiceBase
    {
        public abstract static Core.Resources.Model Model { get; }
    }

    public abstract class ModularResource<T>(ILogger<T> logger, ViamResourceName resourceName) : IModularResource, IAsyncReconfigurable
    {
        protected ILogger<T> Logger = logger;
        public ViamResourceName ResourceName { get; } = resourceName;
        public string ComponentName => ResourceName.Name;

        public virtual string[] ValidateConfig(App.V1.ComponentConfig config) => Array.Empty<string>();
        public virtual ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies) => ValueTask.CompletedTask;
        public virtual ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command, TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<IDictionary<string, object?>>(new Dictionary<string, object?>());

        public virtual ValueTask StopResource() => ValueTask.CompletedTask;
        public virtual ResourceStatus GetStatus() => throw new NotImplementedException();
        public virtual void Dispose() { }
    }
}
