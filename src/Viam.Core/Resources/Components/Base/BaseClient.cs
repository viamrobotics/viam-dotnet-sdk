using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Base.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Base
{
    public class BaseClient(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<BaseClient, Component.Base.V1.BaseService.BaseServiceClient>(resourceName,
                new Component.Base.V1.BaseService.BaseServiceClient(channel)),
            IBase
    {
        public static SubType SubType = SubType.FromRdkComponent("base");

        public static IBase FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<BaseClient>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(
                        new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var results = res.Result.ToDictionary();
                logger.LogMethodInvocationSuccess(results: results);
                return results;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask MoveStraight(long distance,
            double velocity,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, distance, velocity]);
                await Client.MoveStraightAsync(
                        new MoveStraightRequest()
                        {
                            Name = Name,
                            DistanceMm = distance,
                            MmPerSec = velocity,
                            Extra = extra?.ToStruct()
                        },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask Spin(double angle,
            double velocity,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, angle, velocity]);
                await Client.SpinAsync(new SpinRequest()
                        {
                            Name = Name,
                            AngleDeg = angle,
                            DegsPerSec = velocity,
                            Extra = extra?.ToStruct()
                        },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask SetPower(Vector3 linear,
            Vector3 angular,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, linear, angular]);
                await Client
                    .SetPowerAsync(
                        new SetPowerRequest()
                            { Name = Name, Linear = linear, Angular = angular, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask SetVelocity(Vector3 linear,
            Vector3 angular,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, linear, angular]);
                await Client.SetVelocityAsync(new SetVelocityRequest()
                        {
                            Name = Name,
                            Linear = linear,
                            Angular = angular,
                            Extra = extra?.ToStruct()
                        },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask Stop(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.IsMoving);

                return res.IsMoving;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<BaseProperties> GetProperties(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPropertiesAsync(
                        new GetPropertiesRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var properties = new BaseProperties(res.TurningRadiusMeters, res.WheelCircumferenceMeters,
                    res.WidthMeters);
                logger.LogMethodInvocationSuccess(results: properties);
                return properties;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                    .GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var geometries = res.Geometries.ToArray();
                logger.LogMethodInvocationSuccess(results: geometries);
                return geometries;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}