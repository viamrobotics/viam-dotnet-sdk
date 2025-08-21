using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Viam.Common.V1;
using Viam.Component.Encoder.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

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

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => new ValueTask();

        public override async ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client
                    .DoCommandAsync(
                        new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result.ToDictionary();
                Logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask ResetPosition(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.ResetPositionAsync(new ResetPositionRequest() { Name = Name, Extra = extra?.ToStruct() },
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
            IDictionary<string, object?>? extra = null,
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
                    Extra = extra?.ToStruct()
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


        public async ValueTask<EncoderProperties> GetProperties(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPropertiesAsync(
                        new GetPropertiesRequest() { Name = Name, Extra = extra?.ToStruct() },
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


        public async ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: Name);
                var res = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = Name, Extra = extra?.ToStruct() },
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