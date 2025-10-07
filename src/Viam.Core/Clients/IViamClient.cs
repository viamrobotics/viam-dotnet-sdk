using System;
using Viam.Core.App;

namespace Viam.Core.Clients
{
    public interface IViamClient : IAsyncDisposable
    {
        public AppClient CreateAppClient();

        public DataClient CreateDataClient();

        public DataSyncClient CreateDataSyncClient();

        public BillingClient CreateBillingClient();
    }
}
