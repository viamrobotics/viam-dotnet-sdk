using Grpc.Core;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Core.Clients;
using Viam.Contracts;
using Viam.Contracts.Resources;


namespace Viam.Core.Resources.Components
{
    public interface IComponentBase : IResourceBase
    {
        public ValueTask<Struct?> DoCommand(Struct command,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }

    public abstract class ComponentBase<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TClient>(ViamResourceName resourceName, TClient client, ILogger<T> logger)
        : ComponentBase(resourceName, logger) where T : ComponentBase where TClient : ClientBase<TClient>
    {
        public TClient Client = client;

        public abstract ValueTask StopResource();

        public virtual async ValueTask<Struct?> DoCommand(Struct command,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var cmd = typeof(TClient).GetMethod(nameof(DoCommand));
                if (cmd == null)
                {
                    throw new Exception("Method not found");
                }

                var result = cmd.Invoke(Client,
                [
                    new DoCommandRequest() { Name = ResourceName.Name, Command = command }, null,
                    timeout.ToDeadline(), cancellationToken
                ]);
                if (result is not AsyncUnaryCall<DoCommandResponse> r)
                {
                    throw new Exception("Invalid response");
                }

                var res = await r;

                if (res is not null)
                {
                    var response = res.Result;
                    return response;
                }
                return null;
            }
            catch (Exception)
            {
                // TODO: We should probably wrap the exception before re-throwing it.
                throw;
            }
        }
    }

    public abstract class ComponentBase(ViamResourceName resourceName, ILogger logger) : ResourceBase(resourceName, logger);

    public interface IComponentClient<TClient>
        where TClient : IComponentBase
    {
        static abstract Task<TClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null,
            CancellationToken token = default);

        static abstract TClient FromDependencies(Dependencies dependencies, string name);
    }
}