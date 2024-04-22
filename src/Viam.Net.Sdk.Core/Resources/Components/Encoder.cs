using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Encoder(ResourceName resourceName, ViamChannel channel) : ComponentBase<Encoder, EncoderService.EncoderServiceClient>(resourceName, new EncoderService.EncoderServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Encoder(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("encoder");
        public static Encoder FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Encoder>(resourceName);
        }

        public override async ValueTask<IDictionary<string, object?>> DoCommandAsync(IDictionary<string, object> command,
            TimeSpan? timeout = null)
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
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
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
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);
            return (res.Value, res.PositionType);
        }

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

        public record Properties(bool AngleDegreesSupported, bool TicksCountSupported);
    }
}
