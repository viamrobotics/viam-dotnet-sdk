using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Servo
{
    public interface IServo : IComponentBase
    {
        ValueTask Move(uint angle,
                       Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default,
                       [CallerMemberName] string? caller = null);

        ValueTask<uint> GetPosition(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default,
                                    [CallerMemberName] string? caller = null);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default,
                                 [CallerMemberName] string? caller = null);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default,
                       [CallerMemberName] string? caller = null);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default,
                                            [CallerMemberName] string? caller = null);

    }
}
