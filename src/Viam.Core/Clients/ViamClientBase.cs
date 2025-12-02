using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Viam.App.Data.V1;
using Viam.App.Datasync.V1;
using Viam.App.V1;
using Viam.Core.App;

namespace Viam.Core.Clients
{
    public class ViamClientBase : IViamClient
    {
        private readonly ILoggerFactory _loggerFactory;
        protected readonly ILogger<ViamClientBase> Logger;
        private readonly ViamChannel _channel;

        protected internal ViamClientBase(ILoggerFactory loggerFactory, ViamChannel channel)
        {
            _loggerFactory = loggerFactory;
            _channel = channel;
            Logger = loggerFactory.CreateLogger<ViamClientBase>();
        }

        //public AppClient CreateAppClient() => new(_loggerFactory.CreateLogger<AppClient>(),
        //    new AppService.AppServiceClient(_channel));

        //public DataClient CreateDataClient() =>
        //    new(_loggerFactory.CreateLogger<DataClient>(), new DataService.DataServiceClient(_channel));

        public DataSyncClient CreateDataSyncClient() =>
            new(_loggerFactory.CreateLogger<DataSyncClient>(), new DataSyncService.DataSyncServiceClient(_channel));

        //public BillingClient CreateBillingClient() => new(_loggerFactory.CreateLogger<BillingClient>(),
        //    new BillingService.BillingServiceClient(_channel));

        public async ValueTask DisposeAsync()
        {
            await CastAndDispose(_loggerFactory);
            await CastAndDispose(_channel);

            GC.SuppressFinalize(this);
            return;

            static async ValueTask CastAndDispose(IDisposable resource)
            {
                if (resource is IAsyncDisposable resourceAsyncDisposable)
                    await resourceAsyncDisposable.DisposeAsync();
                else
                    resource.Dispose();
            }
        }
    }
}