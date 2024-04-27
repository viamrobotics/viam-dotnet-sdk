using System.Collections.Generic;
using System.Threading.Tasks;

using Grpc.Core;

using Viam.App.V1;

namespace Viam.Core.Resources
{
    public interface IAsyncReconfigurable
    {
        public ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies);
    }
    public interface IComponentBase : IResourceBase;

    public abstract class ComponentBase<T, TClient>(ViamResourceName resourceName, TClient client) : ComponentBase(resourceName) where T : ComponentBase where TClient : ClientBase<TClient>
    {
        public TClient Client => client;
    }

    public abstract class ComponentBase(ViamResourceName resourceName) : ResourceBase(resourceName);
}
