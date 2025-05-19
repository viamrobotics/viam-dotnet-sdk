using Microsoft.Extensions.Logging;
using Viam.Service.Mlmodel.V1;

namespace Viam.Core.App
{
    public class MlTrainingClient(ILogger<MlTrainingClient> logger, MLModelService.MLModelServiceClient client)
    {
    }
}