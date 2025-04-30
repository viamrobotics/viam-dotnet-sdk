using System;
using System.Threading.Tasks;

using Grpc.Core;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Base.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Base
{
    internal class BaseService(ILogger<BaseService> logger) : Component.Base.V1.BaseService.BaseServiceBase, IServiceBase
    {
        public static Service ServiceName => Service.BaseService;
        public static SubType SubType { get; } = SubType.Base;

        public override async Task<StopResponse> Stop(StopRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBase)context.UserState["resource"];
                await resource.Stop(request.Extra?.ToDictionary(), context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
                var response = new StopResponse();
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
                var resource = (IBase)context.UserState["resource"];
                var res = await resource.DoCommand(request.Command.ToDictionary(),
                                             context.Deadline.ToTimeout(),
                                             context.CancellationToken).ConfigureAwait(false);

                var response = new DoCommandResponse() { Result = res.ToStruct() };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<IsMovingResponse> IsMoving(IsMovingRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBase)context.UserState["resource"];
                var res = await resource.IsMoving(context.Deadline.ToTimeout(), context.CancellationToken).ConfigureAwait(false);
                var response = new IsMovingResponse() { IsMoving = res };
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
                var resource = (IBase)context.UserState["resource"];
                var res = await resource.GetGeometries(request.Extra?.ToDictionary(),
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

        public override async Task<GetPropertiesResponse> GetProperties(GetPropertiesRequest request,
                                                                  ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBase)context.UserState["resource"];
                var res = await resource.GetProperties(request.Extra?.ToDictionary(),
                                                       context.Deadline.ToTimeout(),
                                                       context.CancellationToken).ConfigureAwait(false);

                var response = new GetPropertiesResponse()
                {
                    TurningRadiusMeters = res.TurningRadius,
                    WheelCircumferenceMeters = res.WheelCircumference,
                    WidthMeters = res.Width
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

        public override async Task<MoveStraightResponse> MoveStraight(MoveStraightRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBase)context.UserState["resource"];
                await resource.MoveStraight(request.DistanceMm,
                                            request.MmPerSec,
                                            request.Extra?.ToDictionary(),
                                            context.Deadline.ToTimeout(),
                                            context.CancellationToken).ConfigureAwait(false);

                var response = new MoveStraightResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<SetPowerResponse> SetPower(SetPowerRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBase)context.UserState["resource"];
                await resource.SetPower(request.Linear,
                                        request.Angular,
                                        request.Extra?.ToDictionary(),
                                        context.Deadline.ToTimeout(),
                                        context.CancellationToken).ConfigureAwait(false);
                var response = new SetPowerResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<SetVelocityResponse> SetVelocity(SetVelocityRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBase)context.UserState["resource"];
                await resource.SetVelocity(request.Linear,
                                           request.Angular,
                                           request.Extra?.ToDictionary(),
                                           context.Deadline.ToTimeout(),
                                           context.CancellationToken).ConfigureAwait(false);
                var response = new SetVelocityResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<SpinResponse> Spin(SpinRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBase)context.UserState["resource"];
                await resource.Spin(request.AngleDeg,
                                    request.DegsPerSec,
                                    request.Extra?.ToDictionary(),
                                    context.Deadline.ToTimeout(),
                                    context.CancellationToken).ConfigureAwait(false);

                var response = new SpinResponse();
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
