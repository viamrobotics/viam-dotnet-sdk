using Grpc.Core;

using Viam.Common.V1;

namespace Viam.Net.Sdk.Core.Resources
{
    public abstract class ComponentBase<T, TClient>(ResourceName resourceName, TClient client) : ComponentBase where T : ComponentBase where TClient : ClientBase<TClient>
    {
        public ResourceName ResourceName => resourceName;
        public string Name => ResourceName.Name;
        public TClient Client => client;
    }

    public abstract class ComponentBase : ResourceBase
    {
    }
}
