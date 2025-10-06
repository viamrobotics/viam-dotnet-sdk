using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Viam.Core.Clients;

namespace Viam.Core.Resources.Components.Sensor
{
    public interface ISensor : IComponentBase
    {
        public ValueTask<Dictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface ISensorClient : ISensor;
}