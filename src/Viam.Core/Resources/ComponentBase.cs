using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Core.Utils;

namespace Viam.Core.Resources
{
    public interface IComponentBase : IResourceBase
    {
    }

    public abstract class ComponentBase<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)] TClient>(
        ViamResourceName resourceName, TClient client) 
        : ComponentBase(resourceName) where T : ComponentBase where TClient : ClientBase<TClient>
    {
        public TClient Client = client;

        public override async ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
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
                    new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() }, null,
                        timeout.ToDeadline(), cancellationToken
                ]);
                if (result is not AsyncUnaryCall<DoCommandResponse> r)
                {
                    throw new Exception("Invalid response");
                }

                var res = await r;

                var response = res.Result.ToDictionary();
                return response;
            }
            catch (Exception)
            {
                // TODO: We should probably wrap the exception before re-throwing it.
                throw;
            }
        }
    }

    public abstract class ComponentBase(ViamResourceName resourceName) : ResourceBase(resourceName);
}