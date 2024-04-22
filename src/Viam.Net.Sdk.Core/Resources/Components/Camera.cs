using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

using Viam.Common.V1;
using Viam.Component.Camera.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Camera(ResourceName resourceName, ViamChannel channel) : ComponentBase<Camera, CameraService.CameraServiceClient>(resourceName, new CameraService.CameraServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Camera(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("camera");
        public static Camera FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Camera>(resourceName);
        }

        public override async ValueTask<IDictionary<string, object?>> DoCommandAsync(IDictionary<string, object> command,
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
    }
    
    public abstract record MimeType(string Name);

    public record Unsupported() : MimeType("unsupported");

    public record ViamRgba() : MimeType("image/vnd.viam.rgba");

    public record ViamRawDepth() : MimeType("image/vnd.viam.dep");

    public record Jpeg() : MimeType("image/jpeg");

    public record Png() : MimeType("image/png");

    public record Pcd() : MimeType("pointcloud/pcd");
}
