using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Movementsensor.V1;

namespace Viam.Core.Resources.Services
{
    internal class MovementSensor : MovementSensorService.MovementSensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.movementsensor.v1.MovementSensorService";
        public override Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request, ServerCallContext context) => base.GetProperties(request, context);
        public override Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context) => base.DoCommand(request, context);
        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request, ServerCallContext context) => base.GetGeometries(request, context);
        public override Task<GetPositionResponse> GetPosition(GetPositionRequest request, ServerCallContext context) => base.GetPosition(request, context);
        public override Task<GetReadingsResponse> GetReadings(GetReadingsRequest request, ServerCallContext context) => base.GetReadings(request, context);
        public override Task<GetAccuracyResponse> GetAccuracy(GetAccuracyRequest request, ServerCallContext context) => base.GetAccuracy(request, context);
        public override Task<GetAngularVelocityResponse> GetAngularVelocity(GetAngularVelocityRequest request, ServerCallContext context) => base.GetAngularVelocity(request, context);
        public override Task<GetCompassHeadingResponse> GetCompassHeading(GetCompassHeadingRequest request, ServerCallContext context) => base.GetCompassHeading(request, context);
        public override Task<GetLinearAccelerationResponse> GetLinearAcceleration(GetLinearAccelerationRequest request, ServerCallContext context) => base.GetLinearAcceleration(request, context);
        public override Task<GetLinearVelocityResponse> GetLinearVelocity(GetLinearVelocityRequest request, ServerCallContext context) => base.GetLinearVelocity(request, context);
        public override Task<GetOrientationResponse> GetOrientation(GetOrientationRequest request, ServerCallContext context) => base.GetOrientation(request, context);
    }
}
