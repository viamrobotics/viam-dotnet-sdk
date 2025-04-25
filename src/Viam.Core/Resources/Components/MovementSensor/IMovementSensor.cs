using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Core.Resources.Components.Sensor;

namespace Viam.Core.Resources.Components.MovementSensor
{
    public interface IMovementSensor : ISensor
    {
        ValueTask<(GeoPoint, float)> GetPosition(IDictionary<string, object?>? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default);

        ValueTask<Vector3> GetLinearVelocity(IDictionary<string, object?>? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default);

        ValueTask<Vector3> GetAngularVelocity(IDictionary<string, object?>? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default);

        ValueTask<Vector3> GetLinearAcceleration(IDictionary<string, object?>? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default);

        ValueTask<double> GetCompassHeading(IDictionary<string, object?>? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

        ValueTask<Orientation> GetOrientation(IDictionary<string, object?>? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default);

        ValueTask<MovementSensorClient.Properties> GetProperties(IDictionary<string, object?>? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default);

        ValueTask<MovementSensorClient.Accuracy> GetAccuracy(IDictionary<string, object?>? extra = null,
                                                       TimeSpan? timeout = null,
                                                       CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }
}
