using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Generic
{
    public interface IGeneric : IComponentBase
    {
        ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface IGenericClient : IGeneric;
}