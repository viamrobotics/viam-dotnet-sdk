using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Viam.App.V1;
using Viam.Service.Mlmodel.V1;

namespace Viam.Core.App
{
    public class MlTrainingClient(ILogger<MlTrainingClient> logger, MLModelService.MLModelServiceClient client)
    {
    }
}
