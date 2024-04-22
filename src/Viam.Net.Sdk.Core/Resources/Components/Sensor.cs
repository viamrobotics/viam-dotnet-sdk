using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Viam.Common.V1;
using Viam.Component.Sensor.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Sensor(ResourceName resourceName, ViamChannel channel) : ComponentBase<Sensor, SensorService.SensorServiceClient>(resourceName, new SensorService.SensorServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Sensor(name, channel), () => null));

        public static SubType SubType = SubType.FromRdkComponent("sensor");

        public static Sensor FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Sensor>(resourceName);
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

        public async ValueTask<IDictionary<string, object?>> GetReadingsAsync(
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .GetReadingsAsync(new GetReadingsRequest() { Name = ResourceName.Name, Extra = extra },
                                              deadline: timeout.ToDeadline(),
                                              cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Readings.ToDictionary();
        }
    }
}

