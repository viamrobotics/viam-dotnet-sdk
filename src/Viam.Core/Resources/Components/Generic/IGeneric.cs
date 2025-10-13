using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Contracts;

namespace Viam.Core.Resources.Components.Generic
{
    public interface IGeneric : IComponentBase
    {
        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface IGenericClient : IGeneric;
}