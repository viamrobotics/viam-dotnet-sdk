using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Encoder.V1;

namespace Viam.Core.Resources.Components.Encoder
{
    public interface IEncoder : IComponentBase
    {
        ValueTask ResetPosition(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<(float Position, PositionType PositionType)> GetPosition(PositionType? positionType = null,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<EncoderProperties> GetProperties(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface IEncoderClient : IEncoder;
}