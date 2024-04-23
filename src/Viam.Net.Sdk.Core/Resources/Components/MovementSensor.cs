using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Movementsensor.V1;
using Viam.Core.Clients;
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
    }

    public class MovementSensor(ResourceName resourceName, ViamChannel channel)
        : ComponentBase<MovementSensor, MovementSensorService.MovementSensorServiceClient>(
              resourceName,
              new MovementSensorService.MovementSensorServiceClient(channel)), IMovementSensor
    {
        public static SubType SubType = SubType.FromRdkComponent("movement_sensor");

        internal static void RegisterType() => Registry.RegisterSubtype(
            new ResourceRegistration(SubType, (name, channel) => new MovementSensor(name, channel)));

        public static MovementSensor FromRobot(RobotClient client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<MovementSensor>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        internal override ValueTask StopResource() => ValueTask.CompletedTask;

        public override async ValueTask<IDictionary<string, object?>> DoCommand(
            IDictionary<string, object> command,
            TimeSpan? timeout = null)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
                                                  {
                                                      Name = ResourceName.Name, Command = command.ToStruct()
                                                  });

            return res.Result.ToDictionary();
        }

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
