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

namespace Viam.Core.Resources.Components
{
    public interface IMovementSensor : ISensor
    {
        ValueTask<(GeoPoint, float)> GetPosition(Struct? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default);

        ValueTask<Vector3> GetLinearVelocity(Struct? extra = null,
                                             TimeSpan? timeout = null,
                                             CancellationToken cancellationToken = default);

        ValueTask<Vector3> GetAngularVelocity(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default);

        ValueTask<Vector3> GetLinearAcceleration(Struct? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default);

        ValueTask<double> GetCompassHeading(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

        ValueTask<Orientation> GetOrientation(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default);

        ValueTask<MovementSensor.Properties> GetProperties(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);

        ValueTask<MovementSensor.Accuracy> GetAccuracy(Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }

    public class MovementSensor(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<MovementSensor, MovementSensorService.MovementSensorServiceClient>(
              resourceName,
              new MovementSensorService.MovementSensorServiceClient(channel)), IMovementSensor
    {
        static MovementSensor() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new MovementSensor(name, channel, logger)));
        public static SubType SubType = SubType.FromRdkComponent("movement_sensor");

        [LogInvocation]
        public static MovementSensor FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<MovementSensor>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        [LogInvocation]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(
            IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .DoCommandAsync(
                                new DoCommandRequest() { Name = ResourceName.Name, Command = command.ToStruct() },
                                deadline: timeout.ToDeadline(),
                                cancellationToken: cancellationToken)
                            .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogInvocation]
        public async ValueTask<(GeoPoint, float)> GetPosition(Struct? extra = null,
                                                              TimeSpan? timeout = null,
                                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPositionAsync(new GetPositionRequest() { Name = Name, Extra = extra },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return (res.Coordinate, res.AltitudeM);
        }

        [LogInvocation]
        public async ValueTask<Vector3> GetLinearVelocity(Struct? extra = null,
                                                          TimeSpan? timeout = null,
                                                          CancellationToken cancellationToken = default)
        {
            var res = await Client.GetLinearVelocityAsync(new GetLinearVelocityRequest() { Name = Name, Extra = extra },
                                                          deadline: timeout.ToDeadline(),
                                                          cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.LinearVelocity;
        }

        [LogInvocation]
        public async ValueTask<Vector3> GetAngularVelocity(Struct? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default)
        {
            var res = await Client.GetAngularVelocityAsync(
                                      new GetAngularVelocityRequest() { Name = Name, Extra = extra },
                                      deadline: timeout.ToDeadline(),
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.AngularVelocity;
        }

        [LogInvocation]
        public async ValueTask<Vector3> GetLinearAcceleration(Struct? extra = null,
                                                              TimeSpan? timeout = null,
                                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.GetLinearAccelerationAsync(
                                      new GetLinearAccelerationRequest() { Name = Name, Extra = extra, },
                                      deadline: timeout.ToDeadline(),
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.LinearAcceleration;
        }

        [LogInvocation]
        public async ValueTask<double> GetCompassHeading(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetCompassHeadingAsync(
                                      new GetCompassHeadingRequest() { Name = Name, Extra = extra, },
                                      deadline: timeout.ToDeadline(),
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Value;
        }

        [LogInvocation]
        public async ValueTask<Orientation> GetOrientation(Struct? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default)
        {
            var res = await Client.GetOrientationAsync(new GetOrientationRequest() { Name = Name, Extra = extra, },
                                                       deadline: timeout.ToDeadline(),
                                                       cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Orientation;
        }

        [LogInvocation]
        public async ValueTask<Properties> GetProperties(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetPropertiesAsync(new GetPropertiesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return new Properties(res.AngularVelocitySupported,
                                  res.LinearVelocitySupported,
                                  res.LinearAccelerationSupported,
                                  res.CompassHeadingSupported,
                                  res.OrientationSupported,
                                  res.PositionSupported);
        }

        [LogInvocation]
        public async ValueTask<Accuracy> GetAccuracy(Struct? extra = null,
                                                     TimeSpan? timeout = null,
                                                     CancellationToken cancellationToken = default)
        {
            var res = await Client.GetAccuracyAsync(new GetAccuracyRequest() { Name = Name, Extra = extra, },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return new Accuracy(res.Accuracy.ToDictionary(x => x.Key, x => x.Value),
                                res.HasCompassDegreesError,
                                res.CompassDegreesError,
                                res.HasPositionHdop,
                                res.PositionHdop,
                                res.HasPositionNmeaGgaFix,
                                res.PositionNmeaGgaFix,
                                res.HasPositionVdop,
                                res.PositionVdop);
        }

        [LogInvocation]
        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(
                                      new GetGeometriesRequest() { Name = ResourceName.Name, Extra = extra },
                                      deadline: timeout.ToDeadline(),
                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Geometries.ToArray();
        }

        [LogInvocation]
        public async ValueTask<IDictionary<string, object?>> GetReadings(Struct? extra = null,
                                                                         TimeSpan? timeout = null,
                                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetReadingsAsync(new GetReadingsRequest() { Name = Name, Extra = extra, },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Readings.ToDictionary();
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
