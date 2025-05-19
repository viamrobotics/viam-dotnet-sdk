using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Servo
{
    public interface IServo : IComponentBase
    {
        ValueTask Move(uint angle,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null);

        ValueTask<uint> GetPosition(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null);

        ValueTask Stop(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null);

        ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default,
            [CallerMemberName] string? caller = null);
    }
}