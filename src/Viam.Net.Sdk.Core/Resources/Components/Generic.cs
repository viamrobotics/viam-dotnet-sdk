using Viam.Common.V1;
using Viam.Component.Generic.V1;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public abstract class Generic(ResourceName resourceName, ViamChannel channel) : ComponentBase<Generic, GenericService.GenericServiceClient>(resourceName, new GenericService.GenericServiceClient(channel))
    {
    }
}
