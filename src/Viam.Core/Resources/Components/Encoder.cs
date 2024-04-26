using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IEncoder : IComponentBase
    {
        ValueTask ResetPosition(Struct? extra = null,
                                TimeSpan? timeout = null,
                                CancellationToken cancellationToken = default);

        ValueTask<(float Position, PositionType PositionType)> GetPosition(PositionType? positionType = null,
                                                     Struct? extra = null,
                                                     TimeSpan? timeout = null,
                                                     CancellationToken cancellationToken = default);

        ValueTask<Encoder.Properties> GetProperties(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }

    public class Encoder(ResourceName resourceName, ViamChannel channel) : ComponentBase<Encoder, EncoderService.EncoderServiceClient>(resourceName, new EncoderService.EncoderServiceClient(channel)), IEncoder
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Encoder(name, channel), manager => new Services.Encoder()));
        public static SubType SubType = SubType.FromRdkComponent("encoder");
        public static Encoder FromRobot(RobotClientBase client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Encoder>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
                                                         {
                                                             Name = ResourceName.Name, Command = command.ToStruct()
                                                         });

            return res.Result.ToDictionary();
        }

        public async ValueTask ResetPosition(Struct? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default)
        {
            await Client.ResetPositionAsync(new ResetPositionRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken);
        }

        public async ValueTask<(float, PositionType)> GetPosition(PositionType? positionType = null,
                                                                  Struct? extra = null,
                                                                  TimeSpan? timeout = null,
                                                                  CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPositionAsync(new GetPositionRequest()
                                                    {
                                                        Name = Name,
                                                        PositionType =
                                                            positionType.GetValueOrDefault(PositionType.Unspecified),
                                                        Extra = extra
                                                    },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken);
            return (res.Value, res.PositionType);
        }

        public async ValueTask<Properties> GetProperties(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken);

            return new Properties(res.AngleDegreesSupported, res.TicksCountSupported);
        }

        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken);

            return res.Geometries.ToArray();
        }

        public record Properties(bool AngleDegreesSupported, bool TicksCountSupported);
    }
}
