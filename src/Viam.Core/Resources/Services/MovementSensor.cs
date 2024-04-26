using System.Threading.Tasks;

using Grpc.Core;

using Viam.Common.V1;
using Viam.Component.Movementsensor.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class MovementSensor : MovementSensorService.MovementSensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.movementsensor.v1.MovementSensorService";

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetProperties(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetPropertiesResponse()
            {
                AngularVelocitySupported = res.AngularVelocitySupported,
                CompassHeadingSupported = res.CompassHeadingSupported,
                LinearAccelerationSupported = res.LinearAccelerationSupported,
                LinearVelocitySupported = res.LinearVelocitySupported,
                OrientationSupported = res.OrientationSupported,
                PositionSupported = res.PositionSupported
            };
        }
        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline.ToTimeout(),
                                               context.CancellationToken);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetGeometries(request.Extra,
                                                   context.Deadline.ToTimeout(),
                                                   context.CancellationToken);

            return new GetGeometriesResponse() { Geometries = { res } };
        }
        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetPosition(request.Extra,
                                                 context.Deadline.ToTimeout(),
                                                 context.CancellationToken);

            return new GetPositionResponse() { AltitudeM = res.Item2, Coordinate = res.Item1 };
        }
        public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetReadings(request.Extra,
                                                 context.Deadline.ToTimeout(),
                                                 context.CancellationToken);

            var resp = new GetReadingsResponse() { };
            resp.Readings.Add(res);
            return resp;
        }
        public override async Task<GetAccuracyResponse> GetAccuracy(GetAccuracyRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetAccuracy(request.Extra, context.Deadline.ToTimeout(), context.CancellationToken);
            var resp = new GetAccuracyResponse()
            {
                CompassDegreesError = res.CompassDegreesError,
                PositionHdop = res.PositionHdop,
                PositionNmeaGgaFix = res.PositionNmeaGgaFix,
                PositionVdop = res.PositionVdop
            };
            resp.Accuracy.Add(res.Accuracies);
            return resp;
        }
        public override async Task<GetAngularVelocityResponse> GetAngularVelocity(GetAngularVelocityRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetAngularVelocity(request.Extra,
                                                        context.Deadline.ToTimeout(),
                                                        context.CancellationToken);

            return new GetAngularVelocityResponse() { AngularVelocity = res };
        }
        public override async Task<GetCompassHeadingResponse> GetCompassHeading(GetCompassHeadingRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetCompassHeading(request.Extra,
                                                       context.Deadline.ToTimeout(),
                                                       context.CancellationToken);

            return new GetCompassHeadingResponse() { Value = res };
        }
        public override async Task<GetLinearAccelerationResponse> GetLinearAcceleration(GetLinearAccelerationRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetLinearAcceleration(request.Extra,
                                                           context.Deadline.ToTimeout(),
                                                           context.CancellationToken);

            return new GetLinearAccelerationResponse() { LinearAcceleration = res };
        }
        public override async Task<GetLinearVelocityResponse> GetLinearVelocity(GetLinearVelocityRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetLinearVelocity(request.Extra,
                                                       context.Deadline.ToTimeout(),
                                                       context.CancellationToken);

            return new GetLinearVelocityResponse() { LinearVelocity = res };
        }
        public override async Task<GetOrientationResponse> GetOrientation(GetOrientationRequest request, ServerCallContext context)
        {
            var resource = (IMovementSensor)context.UserState["resource"];
            var res = await resource.GetOrientation(request.Extra,
                                                    context.Deadline.ToTimeout(),
                                                    context.CancellationToken);

            return new GetOrientationResponse() { Orientation = res };
        }
    }
}
