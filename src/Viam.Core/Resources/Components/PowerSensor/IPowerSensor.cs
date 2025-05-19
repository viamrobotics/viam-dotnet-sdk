using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viam.Core.Resources.Components.Sensor;

namespace Viam.Core.Resources.Components.PowerSensor
{
    public interface IPowerSensor : ISensor
    {
        ValueTask<(double, bool)> GetVoltage(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<(double, bool)> GetCurrent(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<double> GetPower(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }
}