using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
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

    public class Encoder(ViamResourceName resourceName, ViamChannel channel, ILogger logger) :
        ComponentBase<Encoder, EncoderService.EncoderServiceClient>(resourceName, new EncoderService.EncoderServiceClient(channel)),
        IEncoder
    {
        static Encoder() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new Encoder(name, channel, logger)));
        public static SubType SubType = SubType.FromRdkComponent("encoder");

        [LogInvocation]
        public static Encoder FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Encoder>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        [LogInvocation]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .DoCommandAsync(
                                new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                                deadline: timeout.ToDeadline(),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogInvocation]
        public async ValueTask ResetPosition(Struct? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default)
        {
            await Client.ResetPositionAsync(new ResetPositionRequest() { Name = Name, Extra = extra },
                                            deadline: timeout.ToDeadline(),
                                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogInvocation]
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
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
            return (res.Value, res.PositionType);
        }

        [LogInvocation]
        public async ValueTask<Properties> GetProperties(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return new Properties(res.AngleDegreesSupported, res.TicksCountSupported);
        }

        [LogInvocation]
        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Geometries.ToArray();
        }

        public record Properties(bool AngleDegreesSupported, bool TicksCountSupported);
    }
}
