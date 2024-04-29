using System.Collections.Generic;
using System.Threading.Tasks;
using Viam.App.V1;

namespace Viam.Core.Resources
{
    public interface IAsyncReconfigurable
    {
        public ValueTask Reconfigure(ComponentConfig config, IDictionary<ViamResourceName, IResourceBase> dependencies);
    }
}
