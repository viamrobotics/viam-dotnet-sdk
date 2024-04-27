﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Viam.Common.V1;
using Viam.Component.Sensor.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface ISensor : IComponentBase
    {
        public ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                                 TimeSpan? timeout = null,
                                                                 CancellationToken cancellationToken = default);
    }

    public class Sensor(ViamResourceName resourceName, ViamChannel channel) : ComponentBase<Sensor, SensorService.SensorServiceClient>(resourceName, new SensorService.SensorServiceClient(channel)), ISensor
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Sensor(name, channel), manager => new Services.Sensor(manager)));

        public static SubType SubType = SubType.FromRdkComponent("sensor");

        public static Sensor FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Sensor>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
                                                                                TimeSpan? timeout = null, 
                                                                                CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(
                          new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                          deadline: timeout.ToDeadline(),
                          cancellationToken: cancellationToken);

            return res.Result.ToDictionary();
        }

        public async ValueTask<IDictionary<string, object?>> GetReadings(
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .GetReadingsAsync(new GetReadingsRequest() { Name = ResourceName.Name, Extra = extra },
                                              deadline: timeout.ToDeadline(),
                                              cancellationToken: cancellationToken)
                            ;

            return res.Readings.ToDictionary();
        }
    }
}

