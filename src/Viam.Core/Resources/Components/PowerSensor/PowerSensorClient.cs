using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Powersensor.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.PowerSensor
{
    public class PowerSensorClient(ViamResourceName resourceName, ViamChannel channel, ILogger<PowerSensorClient> logger) :
        ComponentBase<PowerSensorClient, Component.Powersensor.V1.PowerSensorService.PowerSensorServiceClient>(
            resourceName,
            new Component.Powersensor.V1.PowerSensorService.PowerSensorServiceClient(channel)),
        IPowerSensorClient, IComponentClient<IPowerSensorClient>
    {
        public static SubType SubType = SubType.FromRdkComponent("power_sensor");

        public static async Task<IPowerSensorClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<IPowerSensorClient>(resourceName, timeout, token);
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
                logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client
                    .DoCommandAsync(
                        new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result.ToDictionary();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<(double, bool)> GetVoltage(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetVoltageAsync(
                        new GetVoltageRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: [res.Volts, res.IsAc]);
                return (res.Volts, res.IsAc);
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<(double, bool)> GetCurrent(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetCurrentAsync(
                        new GetCurrentRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: [res.Amperes, res.IsAc]);
                return (res.Amperes, res.IsAc);
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<double> GetPower(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPowerAsync(new GetPowerRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.Watts);
                return res.Watts;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Dictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetReadingsAsync(
                        new GetReadingsRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var readings = res.Readings.ToDictionary();
                logger.LogMethodInvocationSuccess(results: readings);
                return readings;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}