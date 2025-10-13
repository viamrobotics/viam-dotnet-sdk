using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;
using Viam.Contracts;
using Viam.Contracts.Resources;

namespace Viam.Core.Resources.Components.Encoder
{
    public class EncoderClient(ViamResourceName resourceName, ViamChannel channel, ILogger<EncoderClient> logger) :
        ComponentBase<EncoderClient, Component.Encoder.V1.EncoderService.EncoderServiceClient>(resourceName,
            new Component.Encoder.V1.EncoderService.EncoderServiceClient(channel), logger),
        IEncoderClient, IComponentClient<IEncoderClient>
    {
        public static SubType SubType = SubType.FromRdkComponent("encoder");

        public static async Task<IEncoderClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<IEncoderClient>(resourceName, timeout, token);
        }

        public static IEncoderClient FromDependencies(Dependencies dependencies, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            if (!dependencies.TryGetValue(resourceName, out var resource))
            {
                throw new ArgumentException($"Dependency {resourceName} not found");
            }
            if (resource is not IEncoderClient client)
            {
                throw new ArgumentException($"Dependency {resourceName} is not a {nameof(IEncoderClient)}");
            }
            return client;
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
                var res = await Client
                    .DoCommandAsync(
                        new DoCommandRequest() { Name = Name, Command = command },
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


        public async ValueTask ResetPosition(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.ResetPositionAsync(new ResetPositionRequest() { Name = Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<(float, PositionType)> GetPosition(PositionType? positionType = null,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, positionType]);
                var res = await Client.GetPositionAsync(new GetPositionRequest()
                {
                    Name = Name,
                    PositionType =
                                positionType.GetValueOrDefault(
                                    PositionType.Unspecified),
                    Extra = extra
                },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: [res.Value, res.PositionType]);
                return (res.Value, res.PositionType);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<EncoderProperties> GetProperties(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPropertiesAsync(
                        new GetPropertiesRequest() { Name = Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var properties = new EncoderProperties(res.AngleDegreesSupported, res.TicksCountSupported);
                Logger.LogMethodInvocationSuccess(results: properties);
                return properties;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: Name);
                var res = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var geometry = res.Geometries.ToArray();
                Logger.LogMethodInvocationSuccess(results: geometry);
                return geometry;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}