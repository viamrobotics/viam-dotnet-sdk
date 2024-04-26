using System;
using System.Threading.Tasks;
using Grpc.Core;
using Viam.Common.V1;
using Viam.Component.Board.V1;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Services
{
    internal class Board : BoardService.BoardServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.board.v1.BoardService";

        public override async Task<DoCommandResponse> DoCommand(DoCommandRequest request, ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var res = await resource.DoCommand(request.Command.ToDictionary(),
                                               context.Deadline - DateTime.UtcNow,
                                               context.CancellationToken);

            return new DoCommandResponse() { Result = res.ToStruct() };
        }

        public override async Task<GetDigitalInterruptValueResponse> GetDigitalInterruptValue(
            GetDigitalInterruptValueRequest request,
            ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["board"];
            var interrupt = await resource.GetDigitalInterruptByName(request.DigitalInterruptName);

            var val = await interrupt.ValueAsync(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);

            return new GetDigitalInterruptValueResponse() { Value = val };
        }

        public override async Task<GetGPIOResponse> GetGPIO(GetGPIORequest request, ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var pin = await resource.GetGpioPinByName(request.Pin);
            var val = await pin.GetAsync(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new GetGPIOResponse() { High = val };
        }

        public override Task<GetGeometriesResponse> GetGeometries(GetGeometriesRequest request,
                                                                        ServerCallContext context) =>
            throw new NotImplementedException();

        public override async Task<PWMResponse> PWM(PWMRequest request, ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var pin = await resource.GetGpioPinByName(request.Pin);
            var val = await pin.GetPwmAsync(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new PWMResponse() { DutyCyclePct = val };
        }

        public override async Task<PWMFrequencyResponse> PWMFrequency(PWMFrequencyRequest request,
                                                                      ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var pin = await resource.GetGpioPinByName(request.Pin);
            var val = await pin.GetPwmFrequencyAsync(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new PWMFrequencyResponse() { FrequencyHz = val };
        }

        public override async Task<ReadAnalogReaderResponse> ReadAnalogReader(
            ReadAnalogReaderRequest request,
            ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["board"];
            var reader = await resource.GetAnalogReaderByName(request.AnalogReaderName);
            var val = await reader.ReadAsync(request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new ReadAnalogReaderResponse() { Value = val };
        }

        public override async Task<SetGPIOResponse> SetGPIO(SetGPIORequest request, ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var pin = await resource.GetGpioPinByName(request.Pin);
            await pin.SetAsync(request.High, request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new SetGPIOResponse();
        }

        public override async Task<SetPWMResponse> SetPWM(SetPWMRequest request, ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var pin = await resource.GetGpioPinByName(request.Pin);
            await pin.SetPwmAsync(request.DutyCyclePct, request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new SetPWMResponse();
        }

        public override async Task<SetPWMFrequencyResponse> SetPWMFrequency(
            SetPWMFrequencyRequest request,
            ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            var pin = await resource.GetGpioPinByName(request.Pin);
            await pin.SetPwmFrequencyAsync(request.FrequencyHz, request.Extra, context.Deadline - DateTime.UtcNow, context.CancellationToken);
            return new SetPWMFrequencyResponse();
        }

        public override async Task<SetPowerModeResponse> SetPowerMode(SetPowerModeRequest request,
                                                                      ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            await resource.SetPowerModeAsync(request.PowerMode,
                                                       request.Duration.ToTimeSpan(),
                                                       request.Extra,
                                                       context.Deadline - DateTime.UtcNow,
                                                       context.CancellationToken);
            return new SetPowerModeResponse();
        }

        public override Task StreamTicks(StreamTicksRequest request,
                                         IServerStreamWriter<StreamTicksResponse> responseStream,
                                         ServerCallContext context) => throw new NotImplementedException();

        public override async Task<WriteAnalogResponse> WriteAnalog(WriteAnalogRequest request,
                                                                    ServerCallContext context)
        {
            var resource = (IBoard)context.UserState["resource"];
            await resource.WriteAnalogAsync(request.Pin,
                                            request.Value,
                                            request.Extra,
                                            context.Deadline - DateTime.UtcNow,
                                            context.CancellationToken);
            return new WriteAnalogResponse();
        }
    }
}
