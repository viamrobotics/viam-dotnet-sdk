using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Movementsensor.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.MovementSensor
{
    public class MovementSensorClient(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<MovementSensorClient, Component.Movementsensor.V1.MovementSensorService.MovementSensorServiceClient>(
              resourceName,
              new Component.Movementsensor.V1.MovementSensorService.MovementSensorServiceClient(channel)), IMovementSensor
    {
        static MovementSensorClient() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new MovementSensorClient(name, channel, logger)));
        public static SubType SubType = SubType.FromRdkComponent("movement_sensor");


        public static IMovementSensor FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<MovementSensorClient>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => new ValueTask();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
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


        public async ValueTask<(GeoPoint, float)> GetPosition(IDictionary<string, object?>? extra = null,
                                                              TimeSpan? timeout = null,
                                                              CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPositionAsync(new GetPositionRequest() { Name = Name, Extra = extra?.ToStruct() },
                                                        deadline: timeout.ToDeadline(),
                                                        cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: [res.Coordinate, res.AltitudeM]);
                return (res.Coordinate, res.AltitudeM);
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Vector3> GetLinearVelocity(IDictionary<string, object?>? extra = null,
                                                          TimeSpan? timeout = null,
                                                          CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetLinearVelocityAsync(
                                          new GetLinearVelocityRequest() { Name = Name, Extra = extra?.ToStruct() },
                                          deadline: timeout.ToDeadline(),
                                          cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.LinearVelocity);
                return res.LinearVelocity;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Vector3> GetAngularVelocity(IDictionary<string, object?>? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetAngularVelocityAsync(
                                          new GetAngularVelocityRequest() { Name = Name, Extra = extra?.ToStruct() },
                                          deadline: timeout.ToDeadline(),
                                          cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.AngularVelocity);
                return res.AngularVelocity;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Vector3> GetLinearAcceleration(IDictionary<string, object?>? extra = null,
                                                              TimeSpan? timeout = null,
                                                              CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetLinearAccelerationAsync(
                                          new GetLinearAccelerationRequest() { Name = Name, Extra = extra?.ToStruct(), },
                                          deadline: timeout.ToDeadline(),
                                          cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.LinearAcceleration);
                return res.LinearAcceleration;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<double> GetCompassHeading(IDictionary<string, object?>? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetCompassHeadingAsync(
                                          new GetCompassHeadingRequest() { Name = Name, Extra = extra?.ToStruct(), },
                                          deadline: timeout.ToDeadline(),
                                          cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.Value);
                return res.Value;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Orientation> GetOrientation(IDictionary<string, object?>? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetOrientationAsync(new GetOrientationRequest() { Name = Name, Extra = extra?.ToStruct(), },
                                                           deadline: timeout.ToDeadline(),
                                                           cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.Orientation);
                return res.Orientation;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Properties> GetProperties(IDictionary<string, object?>? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra?.ToStruct() },
                                                          deadline: timeout.ToDeadline(),
                                                          cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);

                var props = new Properties(res.AngularVelocitySupported,
                                      res.LinearVelocitySupported,
                                      res.LinearAccelerationSupported,
                                      res.CompassHeadingSupported,
                                      res.OrientationSupported,
                                      res.PositionSupported);
                logger.LogMethodInvocationSuccess(results: props);
                return props;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Accuracy> GetAccuracy(IDictionary<string, object?>? extra = null,
                                                     TimeSpan? timeout = null,
                                                     CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetAccuracyAsync(new GetAccuracyRequest() { Name = Name, Extra = extra?.ToStruct(), },
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
                logger.LogMethodInvocationSuccess(results: accuracy);
                return accuracy;
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
                var res = await Client.GetGeometriesAsync(
                                          new GetGeometriesRequest() { Name = ResourceName.Name, Extra = extra?.ToStruct() },
                                          deadline: timeout.ToDeadline(),
                                          cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);

                var geometry = res.Geometries.ToArray();
                logger.LogMethodInvocationSuccess(results: geometry);
                return geometry;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<IDictionary<string, object?>> GetReadings(IDictionary<string, object?>? extra = null,
                                                                         TimeSpan? timeout = null,
                                                                         CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetReadingsAsync(new GetReadingsRequest() { Name = Name, Extra = extra?.ToStruct(), },
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
