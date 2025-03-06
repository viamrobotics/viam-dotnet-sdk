using Grpc.Core;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Viam.Common.V1;
using Viam.Core.Utils;

namespace Viam.Core.Resources
{
    public interface IComponentBase : IResourceBase
    {
    }

    public abstract class ComponentBase<T, TClient>(ViamResourceName resourceName, TClient client) : ComponentBase(resourceName) where T : ComponentBase where TClient : ClientBase<TClient>
    {
        public TClient Client => client;

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
        TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Abstract Do Command");
            try
            {
                var cmd = Client.GetType()
                                .GetMethod(nameof(DoCommand));
                if (cmd == null)
                {
                    throw new Exception("Method not found");
                }
                var result = cmd.Invoke(Client, new object?[] { new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() }, null, timeout.ToDeadline(), cancellationToken });
                if (result is not AsyncUnaryCall<DoCommandResponse> r)
                {
                    throw new Exception("Invalid response");
                }
                var res = await r;

                var response = res.Result.ToDictionary();
                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

    public abstract class ComponentBase(ViamResourceName resourceName) : ResourceBase(resourceName);
}
