using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Contracts;
using Viam.Contracts.Resources;

namespace Viam.Core.Resources.Components.Sensor
{
    public class SensorClient(ViamResourceName resourceName, ViamChannel channel, ILogger<SensorClient> logger)
        : ComponentBase<SensorClient, Component.Sensor.V1.SensorService.SensorServiceClient>(
                resourceName,
                new Component.Sensor.V1.SensorService.SensorServiceClient(channel),
                logger),
            ISensorClient, IComponentClient<ISensorClient>
    {
        public static SubType SubType = SubType.FromRdkComponent("sensor");

        public static ViamResourceName GetResourceName(string? name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            return new ViamResourceName(SubType, name);
        }

        public static ISensorClient FromDependencies(Dependencies dependencies, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            if (!dependencies.TryGetValue(resourceName, out var resource))
            {
                throw new ArgumentException($"Dependency {resourceName} not found");
            }
            if (resource is not ISensorClient client)
            {
                throw new ArgumentException($"Dependency {resourceName} is not a {nameof(ISensorClient)}");
            }
            return client;
        }

        public static async Task<ISensorClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<ISensorClient>(resourceName, timeout, token);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => new ValueTask();

        public override async ValueTask<Struct> DoCommand(Struct command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(
                        new DoCommandRequest() { Name = ResourceName.Name, Command = command },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result;
                Logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<MapField<string, Value>> GetReadings(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                    .GetReadingsAsync(new GetReadingsRequest() { Name = ResourceName.Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Readings;
                Logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}