using Grpc.Core;

using Viam.Common.V1;

namespace Viam.Core.Resources
{
    public interface IComponentBase : IResourceBase
    {
    }

    public abstract class ComponentBase<T, TClient>(ResourceName resourceName, TClient client) : ComponentBase(resourceName) where T : ComponentBase where TClient : ClientBase<TClient>
    {
        public TClient Client => client;
    }

    public abstract class ComponentBase(ResourceName resourceName) : ResourceBase(resourceName)
    {
    }
}
