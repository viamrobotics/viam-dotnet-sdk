using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Viam.Core.Resources.Components.Sensor
{
    public interface ISensor : IComponentBase
    {
        public ValueTask<IDictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
                                                                   TimeSpan? timeout = null,
                                                                   CancellationToken cancellationToken = default);
    }
}
