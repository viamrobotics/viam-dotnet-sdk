using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    // TODO: Complete this interface when the component is complete
    public interface ICamera : IComponentBase
    {
        ValueTask GetImage(Camera.MimeType mimeType,
                           Struct? extra = null,
                           TimeSpan? timeout = null,
                           CancellationToken cancellationToken = default);

        ValueTask GetImages(TimeSpan? timeout = null,
                            CancellationToken cancellationToken = default);

        ValueTask GetPointCloud();

        ValueTask<Camera.Properties> GetProperties();

        ValueTask<Geometry[]> GetGeometries();
    }
    public class Camera(ResourceName resourceName, ViamChannel channel) : ComponentBase<Camera, CameraService.CameraServiceClient>(resourceName, new CameraService.CameraServiceClient(channel)), ICamera
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Camera(name, channel)));
        public static SubType SubType = SubType.FromRdkComponent("camera");
        public static Camera FromRobot(RobotClient client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Camera>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        internal override ValueTask StopResource() => ValueTask.CompletedTask;

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object> command,
            TimeSpan? timeout = null)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
            {
                Name = Name,
                Command = command.ToStruct()
            });

            return res.Result.ToDictionary();
        }

        public ValueTask GetImage(MimeType mimeType,
                                        Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

        public ValueTask GetImages(TimeSpan? timeout = null,
                                   CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();


        public ValueTask GetPointCloud() => throw new NotImplementedException();

        public ValueTask<Properties> GetProperties() => throw new NotImplementedException();

        public ValueTask<Geometry[]> GetGeometries() => throw new NotImplementedException();

        public record Properties(
            DistortionParameters fooDistortionParameters,
            IntrinsicParameters fooIntrinsicParameters,
            RepeatedField<string> fooMimeTypes,
            bool fooSupportsPcd);

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
