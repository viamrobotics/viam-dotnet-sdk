using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Viam.Core.Resources.Components.Gripper
{
    public interface IGripper : IComponentBase
    {
        ValueTask Open(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask Grab(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask Stop(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }
}