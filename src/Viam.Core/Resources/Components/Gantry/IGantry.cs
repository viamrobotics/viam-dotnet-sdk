using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Gantry
{
    public interface IGantry : IComponentBase
    {
        ValueTask<double[]> GetPosition(Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default);

        ValueTask MoveToPosition(double[] positions,
                                 double[] speeds,
                                 Struct? extra = null,
                                 TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask Home(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<double[]> GetLengths(Struct? extra = null,
                                       TimeSpan? timeout = null,
                                       CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }
}
