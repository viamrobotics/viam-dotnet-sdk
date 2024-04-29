using System;
using System.Threading.Tasks;
using Google.Api;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Camera(ILogger logger) : CameraService.CameraServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.camera.v1.CameraService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (ICamera)context.UserState["resource"];
            var resp = await resource.DoCommand(request.Command.ToDictionary(),
                                                context.Deadline.ToTimeout(),
                                                context.CancellationToken).ConfigureAwait(false);

            return new DoCommandResponse() { Result = resp.ToStruct() };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (ICamera)context.UserState["resource"];
            var resp = await resource.GetGeometries(request.Extra,
                                                    context.Deadline.ToTimeout(),
                                                    context.CancellationToken).ConfigureAwait(false);

            return new GetGeometriesResponse() { Geometries = { resp } };
        }

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (ICamera)context.UserState["resource"];
            var resp = await resource.GetProperties(context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            return new GetPropertiesResponse()
            {
                DistortionParameters = resp.DistortionParameters,
                IntrinsicParameters = resp.IntrinsicParameters,
                MimeTypes = { resp.MimeTypes },
                SupportsPcd = resp.SupportsPcd
            };
        }

        public override async Task<GetImageResponse> GetImage(GetImageRequest request, ServerCallContext context)
        {
            var resource = (ICamera)context.UserState["resource"];
            var resp = await resource.GetImage(
                           Viam.Core.Resources.Components.Camera.MimeType.FromName(request.MimeType),
                           request.Extra,
                           context.Deadline.ToTimeout(),
                           context.CancellationToken).ConfigureAwait(false);

            throw new NotImplementedException();
        }

        public override async Task<GetImagesResponse> GetImages(GetImagesRequest request, ServerCallContext context)
        {
            var resource = (ICamera)context.UserState["resource"];
            var resp = await resource.GetImages(context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        public override async Task<GetPointCloudResponse> GetPointCloud(GetPointCloudRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (ICamera)context.UserState["resource"];
            var resp = await resource.GetPointCloud(request.Extra,
                                                    context.Deadline.ToTimeout(),
                                                    context.CancellationToken).ConfigureAwait(false);
            throw new NotImplementedException();
        }

        public override async Task<HttpBody> RenderFrame(RenderFrameRequest request, ServerCallContext context)
        {
            var resource = (ICamera)context.UserState["resource"];
            await Task.Yield();
            throw new NotImplementedException();
        }
    }
}
