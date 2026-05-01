using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Movementsensor.V1;
using Viam.Contracts;
using Viam.Contracts.Resources;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.MovementSensor
{
    internal class MovementSensorService(ILogger<MovementSensorService> logger)
        : Component.Movementsensor.V1.MovementSensorService.MovementSensorServiceBase, IComponentServiceBase
    {
        public static Service ServiceName => Service.MovementSensorService;
        public static SubType SubType { get; } = SubType.MovementSensor;

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetProperties(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetPropertiesResponse()
                {
                    AngularVelocitySupported = res.AngularVelocitySupported,
                    CompassHeadingSupported = res.CompassHeadingSupported,
                    LinearAccelerationSupported = res.LinearAccelerationSupported,
                    LinearVelocitySupported = res.LinearVelocitySupported,
                    OrientationSupported = res.OrientationSupported,
                    PositionSupported = res.PositionSupported
                };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.DoCommand(request.Command,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new DoCommandResponse() { Result = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetGeometries(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetGeometriesResponse() { Geometries = { res } };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetPositionResponse> GetPosition(GetPositionRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetPosition(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetPositionResponse() { AltitudeM = res.Item2, Coordinate = res.Item1 };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetReadingsResponse> GetReadings(GetReadingsRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetReadings(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetReadingsResponse();
                response.Readings.Add(res);
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetAccuracyResponse> GetAccuracy(GetAccuracyRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetAccuracy(request.Extra, context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);
                var response = new GetAccuracyResponse()
                {
                    CompassDegreesError = res.CompassDegreesError,
                    PositionHdop = res.PositionHdop,
                    PositionNmeaGgaFix = res.PositionNmeaGgaFix,
                    PositionVdop = res.PositionVdop
                };
                response.Accuracy.Add(res.Accuracies);
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetAngularVelocityResponse> GetAngularVelocity(GetAngularVelocityRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetAngularVelocity(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetAngularVelocityResponse() { AngularVelocity = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetCompassHeadingResponse> GetCompassHeading(GetCompassHeadingRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetCompassHeading(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetCompassHeadingResponse() { Value = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetLinearAccelerationResponse> GetLinearAcceleration(
            GetLinearAccelerationRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetLinearAcceleration(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetLinearAccelerationResponse() { LinearAcceleration = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetLinearVelocityResponse> GetLinearVelocity(GetLinearVelocityRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetLinearVelocity(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetLinearVelocityResponse() { LinearVelocity = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetOrientationResponse> GetOrientation(GetOrientationRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IMovementSensor)context.UserState["resource"];
                var res = await resource.GetOrientation(request.Extra,
                    context.Deadline.ToTimeout(),
                    context.CancellationToken).ConfigureAwait(false);

                var response = new GetOrientationResponse() { Orientation = res };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}