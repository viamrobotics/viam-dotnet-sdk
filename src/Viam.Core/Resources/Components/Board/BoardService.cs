using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Board.V1;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Board
{
    internal class BoardService(ILogger<BoardService> logger) : Component.Board.V1.BoardService.BoardServiceBase, IServiceBase
    {
        public static Service ServiceName => Service.BoardService;
        public static SubType SubType { get; } = SubType.Board;

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline - DateTime.UtcNow,
                                               context.CancellationToken).ConfigureAwait(false);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetDigitalInterruptValueResponse> GetDigitalInterruptValue(
            GetDigitalInterruptValueRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["board"];
                var interrupt = await resource.GetDigitalInterruptByName(request.DigitalInterruptName);

                var val = await interrupt
                                .ValueAsync(request.Extra?.ToDictionary(),
                                            context.Deadline - DateTime.UtcNow,
                                            context.CancellationToken)
                                .ConfigureAwait(false);

                var response = new GetDigitalInterruptValueResponse() { Value = val };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<GetGPIOResponse> GetGPIO(GetGPIORequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                var pin = await resource.GetGpioPinByName(request.Pin);
                var val = await pin.GetAsync(request.Extra?.ToDictionary(),
                                             context.Deadline - DateTime.UtcNow,
                                             context.CancellationToken)
                                   .ConfigureAwait(false);

                var response = new GetGPIOResponse() { High = val };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<PWMResponse> PWM(PWMRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                var pin = await resource.GetGpioPinByName(request.Pin);
                var val = await pin.GetPwmAsync(request.Extra?.ToDictionary(),
                                                context.Deadline - DateTime.UtcNow,
                                                context.CancellationToken)
                                   .ConfigureAwait(false);

                var response = new PWMResponse() { DutyCyclePct = val };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<PWMFrequencyResponse> PWMFrequency(PWMFrequencyRequest request,
                                                                      ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                var pin = await resource.GetGpioPinByName(request.Pin);
                var val = await pin.GetPwmFrequencyAsync(request.Extra?.ToDictionary(),
                                                         context.Deadline - DateTime.UtcNow,
                                                         context.CancellationToken)
                                   .ConfigureAwait(false);

                var response = new PWMFrequencyResponse() { FrequencyHz = val };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<ReadAnalogReaderResponse> ReadAnalogReader(
            ReadAnalogReaderRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["board"];
                var reader = await resource.GetAnalogReaderByName(request.AnalogReaderName);
                var val = await reader
                                .ReadAsync(request.Extra?.ToDictionary(), context.Deadline - DateTime.UtcNow, context.CancellationToken)
                                .ConfigureAwait(false);

                var response = new ReadAnalogReaderResponse() { Value = val };
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<SetGPIOResponse> SetGPIO(SetGPIORequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                var pin = await resource.GetGpioPinByName(request.Pin);
                await pin.SetAsync(request.High,
                                   request.Extra?.ToDictionary(),
                                   context.Deadline - DateTime.UtcNow,
                                   context.CancellationToken)
                         .ConfigureAwait(false);

                var response = new SetGPIOResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<SetPWMResponse> SetPWM(SetPWMRequest request, ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                var pin = await resource.GetGpioPinByName(request.Pin);
                await pin.SetPwmAsync(request.DutyCyclePct,
                                      request.Extra?.ToDictionary(),
                                      context.Deadline - DateTime.UtcNow,
                                      context.CancellationToken)
                         .ConfigureAwait(false);

                var response = new SetPWMResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<SetPWMFrequencyResponse> SetPWMFrequency(
            SetPWMFrequencyRequest request,
            ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                var pin = await resource.GetGpioPinByName(request.Pin);
                await pin.SetPwmFrequencyAsync(request.FrequencyHz,
                                               request.Extra?.ToDictionary(),
                                               context.Deadline - DateTime.UtcNow,
                                               context.CancellationToken)
                         .ConfigureAwait(false);

                var response = new SetPWMFrequencyResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override async Task<SetPowerModeResponse> SetPowerMode(SetPowerModeRequest request,
                                                                      ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                await resource.SetPowerModeAsync(request.PowerMode,
                                                 request.Duration.ToTimeSpan(),
                                                 request.Extra?.ToDictionary(),
                                                 context.Deadline - DateTime.UtcNow,
                                                 context.CancellationToken)
                              .ConfigureAwait(false);

                var response = new SetPowerModeResponse();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }

        public override Task StreamTicks(StreamTicksRequest request,
                                         IServerStreamWriter<StreamTicksResponse> responseStream,
                                         ServerCallContext context) => throw new NotImplementedException();

        public override async Task<WriteAnalogResponse> WriteAnalog(WriteAnalogRequest request,
                                                                    ServerCallContext context)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [request]);
                var resource = (IBoard)context.UserState["resource"];
                await resource.WriteAnalogAsync(request.Pin,
                                                request.Value,
                                                request.Extra?.ToDictionary(),
                                                context.Deadline - DateTime.UtcNow,
                                                context.CancellationToken)
                              .ConfigureAwait(false);

                var response = new WriteAnalogResponse();
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
