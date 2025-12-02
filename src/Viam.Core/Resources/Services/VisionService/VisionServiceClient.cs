using Google.Protobuf;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;
using Viam.Contracts;
using Viam.Contracts.Resources;
using Viam.Service.Vision.V1;

using GetPropertiesRequest = Viam.Service.Vision.V1.GetPropertiesRequest;

namespace Viam.Core.Resources.Services.VisionService
{
    public class VisionServiceClient(ViamResourceName resourceName, ViamChannel channel, ILogger<VisionServiceClient> logger) :
        ServiceBase<VisionServiceClient, Viam.Service.Vision.V1.VisionService.VisionServiceClient>(resourceName, new Viam.Service.Vision.V1.VisionService.VisionServiceClient(channel), logger),
        IVisionServiceClient, IServiceClient<IVisionServiceClient>
    {
        public static SubType SubType = SubType.FromRdkService("vision");

        public override DateTime? LastReconfigured { get; }

        public static async Task<IVisionServiceClient> FromMachine(
            IMachineClient client, 
            string name, 
            TimeSpan? timeout = null,
            CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<IVisionServiceClient>(resourceName, timeout, token);
        }

        public static IVisionServiceClient FromDependencies(Dependencies dependencies, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            if (!dependencies.TryGetValue(resourceName, out var resource))
            {
                throw new ArgumentException($"Dependency {resourceName} not found");
            }
            if (resource is not IVisionServiceClient client)
            {
                throw new ArgumentException($"Dependency {resourceName} is not a {nameof(IVisionServiceClient)}");
            }
            return client;
        }

        public async Task<(Image Image, Classification[] Classifications, Detection[] Detections, PointCloudObject[] Objects)> CaptureAllFromCamera(string cameraName,
            bool returnImage,
            bool returnClassifications,
            bool returnDetections,
            bool returnObjectPointClouds,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.CaptureAllFromCameraAsync(
                    new CaptureAllFromCameraRequest()
                    {
                        Name = Name,
                        CameraName = cameraName,
                        ReturnImage = returnImage,
                        ReturnClassifications = returnClassifications,
                        ReturnDetections = returnDetections,
                        ReturnObjectPointClouds = returnObjectPointClouds,
                        Extra = extra
                    },
                    deadline: timeout.ToDeadline(),
                    cancellationToken: cancellationToken).ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res);
                var classifications = res.Classifications.Count > 0 ? res.Classifications.ToArray() : [];
                var detections = res.Detections.Count > 0 ? res.Detections.ToArray() : [];
                var objects = res.Objects.Count > 0 ? res.Objects.ToArray() : [];
                return (res.Image, classifications, detections, objects);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<Classification[]> GetClassificationsFromCamera(string cameraName,
            int count,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetClassificationsFromCameraAsync(
                        new GetClassificationsFromCameraRequest() { Name = Name, CameraName = cameraName, N = count, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res);
                return res.Classifications.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<Detection[]> GetDetectionsFromCamera(string cameraName,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetDetectionsFromCameraAsync(
                        new GetDetectionsFromCameraRequest() { Name = Name, CameraName = cameraName, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res);
                return res.Detections.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<Classification[]> GetClassifications(ViamImage image,
            int width,
            int height,
            MimeType mimeType,
            int count,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetClassificationsAsync(
                        new GetClassificationsRequest { Name = Name, Image = ByteString.CopyFrom(image.bytes.Span), Width = width, Height = height, MimeType = mimeType.ToGrpc(), N = count, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res);
                return res.Classifications.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<Detection[]> GetDetections(ViamImage image, int width, int height, MimeType mimeType, Struct? extra = null,
            TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetDetectionsAsync(
                        new GetDetectionsRequest { Name = Name, Image = ByteString.CopyFrom(image.bytes.Span), Width = width, Height = height, MimeType = mimeType.ToGrpc(), Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res);
                return res.Detections.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<PointCloudObject[]> GetObjectPointClouds(
            string cameraName,
            MimeType mimeType, 
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetObjectPointCloudsAsync(
                        new GetObjectPointCloudsRequest { Name = Name, CameraName = cameraName, MimeType = mimeType.ToGrpc(), Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res);
                return res.Objects.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async Task<(bool ClassificationsSupported, bool DetectionsSupported, bool ObjectPointCloudsSupported)> GetProperties(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPropertiesAsync(
                        new GetPropertiesRequest { Name = Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res);
                return (res.ClassificationsSupported, res.DetectionsSupported, res.ObjectPointCloudsSupported);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}
