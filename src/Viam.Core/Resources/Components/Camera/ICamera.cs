using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Camera
{
    public interface ICamera : IComponentBase
    {
        ValueTask<ViamImage?> GetImage(MimeType? mimeType = null,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<ViamImage[]?> GetImages(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<ViamImage?> GetPointCloud(MimeType mimeType,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<CameraProperties?> GetProperties(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]?> GetGeometries(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface ICameraClient : ICamera;
}