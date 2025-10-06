using Grpc.Core;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Viam.Core.Clients;

namespace Viam.Core.Resources.Services
{
    public interface IServiceBase : IResourceBase;

    public abstract class ServiceBase<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TClient>(ViamResourceName resourceName, TClient client, ILogger<T> logger)
        : ServiceBase(resourceName, logger) where T : ServiceBase where TClient : ClientBase<TClient>
    {
        public TClient Client = client;
    }

    public abstract class ServiceBase(ViamResourceName resourceName, ILogger logger) : ResourceBase(resourceName, logger);

    public interface IServiceClient<TClient>
        where TClient : IServiceBase
    {
        static abstract Task<TClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null,
            CancellationToken token = default);
    }
}
