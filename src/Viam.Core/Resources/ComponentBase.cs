using Grpc.Core;

namespace Viam.Core.Resources
{
    public interface IComponentBase : IResourceBase;

    public abstract class ComponentBase<T, TClient>(ViamResourceName resourceName, TClient client) : ComponentBase(resourceName) where T : ComponentBase where TClient : ClientBase<TClient>
    {
        public TClient Client => client;
    }

    public abstract class ComponentBase(ViamResourceName resourceName) : ResourceBase(resourceName);
}
