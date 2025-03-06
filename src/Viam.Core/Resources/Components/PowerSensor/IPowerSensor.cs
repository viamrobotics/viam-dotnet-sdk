using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Core.Resources.Components.Sensor;

namespace Viam.Core.Resources.Components.PowerSensor
{
    public interface IPowerSensor : ISensor
    {
        ValueTask<(double, bool)> GetVoltage(Struct? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default);

        ValueTask<(double, bool)> GetCurrent(Struct? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default);

        ValueTask<double> GetPower(Struct? extra = null,
                                   TimeSpan? timeout = null,
                                   CancellationToken cancellationToken = default);
    }
}
