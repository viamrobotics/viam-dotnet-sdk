using Google.Protobuf.WellKnownTypes;
using Viam.App.V1;
using Viam.Contracts.Resources;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;
using Model = Viam.Contracts.Resources.Model;

namespace Viam.ModularResources
{
    public interface IModularResource : IResourceBase, IAsyncDisposable
    {
        public string Name => ResourceName.Name;
        public string[] ValidateConfig(App.V1.ComponentConfig config);
    }

    public interface IModularResourceService : IModularResource, IComponentServiceBase
    {
        public static abstract Model Model { get; }
    }

    public abstract class ModularResource<T>(ILogger<T> logger, ViamResourceName resourceName)
        : IModularResource, IAsyncReconfigurable
    {
        protected ILogger<T> Logger = logger;
        public ViamResourceName ResourceName { get; } = resourceName;
        public string ComponentName => ResourceName.Name;

        public virtual string[] ValidateConfig(App.V1.ComponentConfig config) => [];

        public virtual ValueTask Reconfigure(ComponentConfig config, Dependencies dependencies) => ValueTask.CompletedTask;

        public virtual ValueTask<Struct?> DoCommand(Struct command,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult<Struct?>(new Struct());

        public virtual ValueTask StopResource() => ValueTask.CompletedTask;
        public virtual ResourceStatus GetStatus() => throw new NotImplementedException();

        public virtual ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }
    }
}