using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Motor
{
    public interface IMotor : IResourceBase
    {
        ValueTask SetPower(double power,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask GoFor(double rpm,
            double revolutions,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask GoTo(double rpm,
            double positionRevolutions,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask ResetZeroPosition(double offset,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<double> GetPosition(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask Stop(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<(bool IsOn, double PowerPct)> IsPowered(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<MotorClient.Properties> GetProperties(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }
}