using System;
using System.Linq;
using System.Threading.Tasks;

using Google.Api;
using Google.Protobuf;
using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Camera
{
    internal class CameraService(ILogger<CameraService> logger) : Component.Camera.V1.CameraService.CameraServiceBase, IServiceBase
    {
        public static Service ServiceName => Service.CameraService;
        public static SubType SubType { get; } = SubType.Camera;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ICamera)context.UserState["resource"];
                var resp = await resource.DoCommand(request.Command.ToDictionary(),
                                                    context.Deadline.ToTimeout(),
                                                    context.CancellationToken).ConfigureAwait(false);

                var response = new DoCommandResponse() { Result = resp.ToStruct() };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ICamera)context.UserState["resource"];
                var resp = await resource.GetGeometries(request.Extra?.ToDictionary(),
                                                        context.Deadline.ToTimeout(),
                                                        context.CancellationToken).ConfigureAwait(false);

                var response = new GetGeometriesResponse() { Geometries = { resp } };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                        ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ICamera)context.UserState["resource"];
                var resp = await resource.GetProperties(context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
                var response = new GetPropertiesResponse()
                {
                    DistortionParameters = resp.DistortionParameters.ToGrpc(),
                    IntrinsicParameters = resp.IntrinsicParameters.ToGrpc(),
                    MimeTypes = { resp.MimeTypes.Select(x => x.ToString()) },
                    SupportsPcd = resp.SupportsPcd
                };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetImageResponse> GetImage(GetImageRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ICamera)context.UserState["resource"];
                var resp = await resource.GetImage(
                               MimeTypeExtensions.FromGrpc(request.MimeType),
                               request.Extra?.ToDictionary(),
                               context.Deadline.ToTimeout(),
                               context.CancellationToken).ConfigureAwait(false);

                var response = new GetImageResponse()
                {
                    MimeType = request.MimeType,
                    Image = ByteString.CopyFrom(resp.bytes.Span)
                };
                logger.LogMethodInvocationSuccess();
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetImagesResponse> GetImages(GetImagesRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ICamera)context.UserState["resource"];
                var resp = await resource.GetImages(context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
                var response = new GetImagesResponse();
                response.Images.AddRange(resp.Select(image => new Image()
                {
                    Image_ = ByteString.CopyFrom(image.bytes.Span),
                    Format = image.mimeType.ToGrpcFormat(),
                    SourceName = image.sourceName
                }));
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetPointCloudResponse> GetPointCloud(GetPointCloudRequest request,
                                                                        ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ICamera)context.UserState["resource"];
                var resp = await resource.GetPointCloud(
                                             MimeTypeExtensions.FromGrpc(request.MimeType),
                                             request.Extra?.ToDictionary(),
                                             context.Deadline.ToTimeout(),
                                             context.CancellationToken)
                                         .ConfigureAwait(false);

                var response = new GetPointCloudResponse()
                {
                    MimeType = resp.mimeType.ToString(),
                    PointCloud = ByteString.CopyFrom(resp.bytes.Span)
                };
                logger.LogMethodInvocationSuccess();
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<HttpBody> RenderFrame(RenderFrameRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (ICamera)context.UserState["resource"];
                await Task.Yield();
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}
