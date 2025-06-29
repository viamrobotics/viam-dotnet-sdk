using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Camera
{
    public class CameraClient(ViamResourceName resourceName, ViamChannel channel, ILogger<CameraClient> logger)
        : ComponentBase<CameraClient, Component.Camera.V1.CameraService.CameraServiceClient>(resourceName,
                new Component.Camera.V1.CameraService.CameraServiceClient(channel)),
            ICamera
    {
        public static SubType SubType = SubType.FromRdkComponent("camera");

        public static ICamera FromRobot(IMachineClient client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<CameraClient>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => new ValueTask();

        public override async ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(
                        new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result.ToDictionary();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<ViamImage> GetImage(MimeType? mimeType = null,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart();
                var res = await Client
                    .GetImageAsync(
                        new GetImageRequest()
                        {
                            Name = Name,
                            MimeType = mimeType?.ToGrpc() ?? string.Empty,
                            Extra = extra?.ToStruct()
                        },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var (width, height) =
                    Utils.GetImageDimensions(
                        res.Image.Memory.Span, MimeTypeExtensions.FromGrpc(res.MimeType));
                var image = new ViamImage(res.Image.Memory, MimeTypeExtensions.FromGrpc(res.MimeType), width, height);
                logger.LogMethodInvocationSuccess();
                return image;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<ViamImage[]> GetImages(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart();
                var res = await Client
                    .GetImagesAsync(
                        new GetImagesRequest() { Name = Name },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = new ViamImage[res.Images.Count];
                var i = 0;
                foreach (var protoImage in res.Images)
                {
                    var (width, height) =
                        Utils.GetImageDimensions(
                            protoImage.Image_.Memory.Span, MimeTypeExtensions.FromGrpc(protoImage.Format));
                    var image = new ViamImage(protoImage.Image_.Memory, MimeTypeExtensions.FromGrpc(protoImage.Format),
                        width, height);
                    response[i] = image;
                    i++;
                }

                logger.LogMethodInvocationSuccess(results: response.Length);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<ViamImage> GetPointCloud(MimeType mimeType,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart();
                var result = await Client
                    .GetPointCloudAsync(
                        new GetPointCloudRequest()
                        {
                            Name = Name,
                            MimeType = mimeType.ToGrpc(),
                            Extra = extra?.ToStruct()
                        },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = new ViamImage(result.PointCloud.Memory, MimeTypeExtensions.FromGrpc(result.MimeType), 0,
                    0);
                logger.LogMethodInvocationSuccess();
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<CameraProperties> GetProperties(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart();
                Debug.Assert(Client != null);
                var result = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var response = new CameraProperties(
                    new DistortionParameters(result.DistortionParameters.Model,
                        result.DistortionParameters.Parameters.ToArray()),
                    new IntrinsicParameters(result.IntrinsicParameters.CenterXPx,
                        result.IntrinsicParameters.CenterYPx,
                        result.IntrinsicParameters.FocalXPx,
                        result.IntrinsicParameters.FocalYPx,
                        result.IntrinsicParameters.HeightPx,
                        result.IntrinsicParameters.WidthPx),
                    result.MimeTypes.Select(MimeTypeExtensions.FromGrpc)
                        .ToArray(),
                    result.SupportsPcd);
                logger.LogMethodInvocationSuccess();
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart();
                var result = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var response = result.Geometries.ToArray();
                logger.LogMethodInvocationSuccess(results: response.Length);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}