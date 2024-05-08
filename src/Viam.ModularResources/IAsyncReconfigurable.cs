using Viam.App.V1;
using Viam.Core.Resources;

namespace Viam.ModularResources
{
    public interface IAsyncReconfigurable
    {
        public ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies);
    }
}
