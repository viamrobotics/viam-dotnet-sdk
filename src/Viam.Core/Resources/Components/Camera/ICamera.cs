using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Camera
{
    public interface ICamera : IComponentBase
    {
        ValueTask<ViamImage> GetImage(MimeType? mimeType = null,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<ViamImage[]> GetImages(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<ViamImage> GetPointCloud(MimeType mimeType,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<CameraProperties> GetProperties(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface ICameraClient : ICamera;
}