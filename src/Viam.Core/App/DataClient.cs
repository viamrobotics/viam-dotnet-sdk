using Microsoft.Extensions.Logging;
using Viam.App.Data.V1;

namespace Viam.Core.App
{
    public class DataClient(ILogger<DataClient> logger, DataService.DataServiceClient client)
    {
    }
}