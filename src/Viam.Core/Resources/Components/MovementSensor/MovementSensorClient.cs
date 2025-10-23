using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Movementsensor.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Contracts;
using Viam.Contracts.Resources;

namespace Viam.Core.Resources.Components.MovementSensor
{
    public class MovementSensorClient(ViamResourceName resourceName, ViamChannel channel, ILogger<MovementSensorClient> logger)
        : ComponentBase<MovementSensorClient,
                Component.Movementsensor.V1.MovementSensorService.MovementSensorServiceClient>(
                resourceName,
                new Component.Movementsensor.V1.MovementSensorService.MovementSensorServiceClient(channel), logger),
            IMovementSensorClient, IComponentClient<IMovementSensorClient>
    {
        public static SubType SubType = SubType.FromRdkComponent("movement_sensor");

        public static async Task<IMovementSensorClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<IMovementSensorClient>(resourceName, timeout, token);
        }

        public static IMovementSensorClient FromDependencies(Dependencies dependencies, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            if (!dependencies.TryGetValue(resourceName, out var resource))
            {
                throw new ArgumentException($"Dependency {resourceName} not found");
            }
            if (resource is not IMovementSensorClient client)
            {
                throw new ArgumentException($"Dependency {resourceName} is not a {nameof(IMovementSensorClient)}");
            }
            return client;
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => new ValueTask();

        public override async ValueTask<Struct?> DoCommand(
            Struct command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client
                    .DoCommandAsync(
                        new DoCommandRequest() { Name = ResourceName.Name, Command = command },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (res is null)
                {
                    Logger.LogMethodInvocationSuccess(results: null);
                    return null;
                }

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

        public async ValueTask<(GeoPoint, float)> GetPosition(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPositionAsync(
                        new GetPositionRequest() { Name = Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: [res.Coordinate, res.AltitudeM]);
                return (res.Coordinate, res.AltitudeM);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async ValueTask<Vector3> GetLinearVelocity(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetLinearVelocityAsync(
                        new GetLinearVelocityRequest() { Name = Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.LinearVelocity);
                return res.LinearVelocity;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async ValueTask<Vector3> GetAngularVelocity(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetAngularVelocityAsync(
                        new GetAngularVelocityRequest() { Name = Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.AngularVelocity);
                return res.AngularVelocity;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async ValueTask<Vector3> GetLinearAcceleration(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetLinearAccelerationAsync(
                        new GetLinearAccelerationRequest() { Name = Name, Extra = extra, },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.LinearAcceleration);
                return res.LinearAcceleration;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async ValueTask<double> GetCompassHeading(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetCompassHeadingAsync(
                        new GetCompassHeadingRequest() { Name = Name, Extra = extra, },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.Value);
                return res.Value;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
        
        public async ValueTask<Orientation> GetOrientation(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetOrientationAsync(
                        new GetOrientationRequest() { Name = Name, Extra = extra, },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.Orientation);
                return res.Orientation;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async ValueTask<Properties> GetProperties(Struct? extra = null,
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

                var props = new Properties(res.AngularVelocitySupported,
                    res.LinearVelocitySupported,
                    res.LinearAccelerationSupported,
                    res.CompassHeadingSupported,
                    res.OrientationSupported,
                    res.PositionSupported);
                Logger.LogMethodInvocationSuccess(results: props);
                return props;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public async ValueTask<Accuracy> GetAccuracy(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetAccuracyAsync(
                        new GetAccuracyRequest() { Name = Name, Extra = extra, },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var accuracy = new Accuracy(res.Accuracy.ToDictionary(x => x.Key, x => x.Value),
                    res.HasCompassDegreesError,
                    res.CompassDegreesError,
                    res.HasPositionHdop,
                    res.PositionHdop,
                    res.HasPositionNmeaGgaFix,
                    res.PositionNmeaGgaFix,
                    res.HasPositionVdop,
                    res.PositionVdop);
                Logger.LogMethodInvocationSuccess(results: accuracy);
                return accuracy;
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
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = ResourceName.Name, Extra = extra },
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
        
        public async ValueTask<MapField<string, Value>> GetReadings(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            extra ??= new Struct();
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetReadingsAsync(
                        new GetReadingsRequest() { Name = Name, Extra = extra, },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var readings = res.Readings;
                Logger.LogMethodInvocationSuccess(results: readings);
                return readings;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public record Properties(
            bool AngularVelocitySupported,
            bool LinearVelocitySupported,
            bool LinearAccelerationSupported,
            bool CompassHeadingSupported,
            bool OrientationSupported,
            bool PositionSupported);

        public record Accuracy(
            IDictionary<string, float> Accuracies,
            bool HasCompassDegreesError,
            float CompassDegreesError,
            bool HasPositionHdop,
            float PositionHdop,
            bool HasPositionNmeaGgaFix,
            int PositionNmeaGgaFix,
            bool HasPositionVdop,
            float PositionVdop);
    }
}