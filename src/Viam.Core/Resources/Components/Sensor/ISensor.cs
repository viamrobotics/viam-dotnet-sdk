using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace Viam.Core.Resources.Components.Sensor
{
    public interface ISensor : IComponentBase
    {
        public ValueTask<MapField<string, Value>> GetReadings(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface ISensorClient : ISensor;
}