using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Viam.App.Data.V1;
using Viam.App.V1;

namespace Viam.Core.App
{
    public class DataClient(ILogger<DataClient> logger, DataService.DataServiceClient client)
    {
    }
}
