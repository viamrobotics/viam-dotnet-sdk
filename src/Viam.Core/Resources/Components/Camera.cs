using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    // TODO: Complete this interface when the component is complete
    public interface ICamera : IComponentBase
    {
        ValueTask<object> GetImage(Camera.MimeType mimeType,
                           Struct? extra = null,
                           TimeSpan? timeout = null,
                           CancellationToken cancellationToken = default);

        ValueTask<object[]> GetImages(TimeSpan? timeout = null,
                            CancellationToken cancellationToken = default);

        ValueTask<object> GetPointCloud(Struct? extra = null,
                                TimeSpan? timeout = null,
                                CancellationToken cancellationToken = default);

        ValueTask<Camera.Properties> GetProperties(TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }

    public class Camera(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<Camera, CameraService.CameraServiceClient>(resourceName,
                                                                   new CameraService.CameraServiceClient(channel)),
          ICamera
    {
        internal static void RegisterType() => Registry.RegisterSubtype(
            new ResourceRegistration(SubType,
                                             (name, channel, logger) => new Camera(name, channel, logger),
                                             (logger) => new Services.Camera(logger)));

        public static SubType SubType = SubType.FromRdkComponent("camera");

        [LogCall]
        public static Camera FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Camera>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        [LogCall]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogCall]
        public ValueTask<object> GetImage(MimeType mimeType,
                                          Struct? extra = null,
                                          TimeSpan? timeout = null,
                                          CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        [LogCall]
        public ValueTask<object[]> GetImages(TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        [LogCall]
        public ValueTask<object> GetPointCloud(Struct? extra = null,
                                               TimeSpan? timeout = null,
                                               CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        [LogCall]
        public ValueTask<Properties> GetProperties(TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        [LogCall]
        public ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public record Properties(
            DistortionParameters DistortionParameters,
            IntrinsicParameters IntrinsicParameters,
            RepeatedField<string> MimeTypes,
            bool SupportsPcd);

        public record MimeType(string Name)
        {
            public static MimeType FromName(string name)
            {
                return name switch
                       {
                           nameof(Unsupported) => Unsupported,
                           nameof(ViamRgba) => ViamRgba,
                           nameof(ViamRawDepth) => ViamRawDepth,
                           nameof(Jpeg) => Jpeg,
                           nameof(Png) => Png,
                           nameof(Pcd) => Pcd,
                           _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown event type")
                       };
            }

            public static MimeType Unsupported = new("unsupported");
            public static MimeType ViamRgba = new("image/vnd.viam.rgba");
            public static MimeType ViamRawDepth = new("image/vnd.viam.dep");
            public static MimeType Jpeg = new("image/jpeg");
            public static MimeType Png = new("image/png");
            public static MimeType Pcd = new("pointcloud/pcd");
        }
    }
}
