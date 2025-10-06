using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Service.Vision.V1;

namespace Viam.Core.Resources.Services.VisionService
{
    public interface IVisionService : IServiceBase
    {
        Task<(Image Image, Classification[] Classifications, Detection[] Detections, PointCloudObject[] Objects)>
            CaptureAllFromCamera(string cameraName,
                bool returnImage,
                bool returnClassifications,
                bool returnDetections,
                bool returnObjectPointClouds,
                IDictionary<string, object?>? extra = null,
                TimeSpan? timeout = null,
                CancellationToken cancellationToken = default);

        Task<Classification[]> GetClassificationsFromCamera(string cameraName,
            int count,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        Task<Detection[]> GetDetectionsFromCamera(string cameraName,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        Task<Classification[]> GetClassifications(ViamImage image,
            int width,
            int height,
            MimeType mimeType,
            int count,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        Task<Detection[]> GetDetections(ViamImage image,
            int width,
            int height,
            MimeType mimeType,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        Task<PointCloudObject[]> GetObjectPointClouds(
            string cameraName,
            MimeType mimeType,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        Task<(bool ClassificationsSupported, bool DetectionsSupported, bool ObjectPointCloudsSupported)> GetProperties(
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface IVisionServiceClient : IVisionService;
}
