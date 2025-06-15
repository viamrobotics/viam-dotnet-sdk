using System;
using Viam.Core.App;

namespace Viam.Core.Clients
{
    public interface IViamClient : IAsyncDisposable
    {
        public AppClient CreateAppClient();

        public DataClient CreateDataClient();

        public BillingClient CreateBillingClient();

        public MlTrainingClient CreateMlTrainingClient();

        public ProvisioningClient CreateProvisioningClient();
    }
}
