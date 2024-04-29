using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Generic.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IGeneric : IResourceBase
    {
        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }
    public class Generic(ResourceName resourceName, ViamChannel channel, ILogger logger) :
        ComponentBase<Generic, GenericService.GenericServiceClient>(resourceName, new GenericService.GenericServiceClient(channel)),
        IGeneric
    {
        public override DateTime? LastReconfigured { get; }

        [LogCall]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .DoCommandAsync(
                                new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                                deadline: timeout.ToDeadline(),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        public override ValueTask StopResource() => throw new NotImplementedException();

        [LogCall]
        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(
                                      new GetGeometriesRequest() { Name = ResourceName.Name, Extra = extra },
                                      deadline: timeout.ToDeadline(),
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Geometries.ToArray();
        }
    }
}
