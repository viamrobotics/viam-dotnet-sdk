using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;

namespace Viam.Core.Resources.Components.InputController
{
    public interface IInputController : IComponentBase
    {
        ValueTask<InputControllerClient.Control[]> GetControls(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default);

        ValueTask<IDictionary<InputControllerClient.Control, InputControllerClient.Event>> GetEvents(InputControllerClient.Control control,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask RegisterControlCallback(InputControllerClient.Control control,
                                          Struct? extra = null,
                                          TimeSpan? timeout = null,
                                          CancellationToken cancellationToken = default);

        ValueTask TriggerEvent(InputControllerClient.Event @event,
                               Struct? extra = null,
                               TimeSpan? timeout = null,
                               CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }
}
