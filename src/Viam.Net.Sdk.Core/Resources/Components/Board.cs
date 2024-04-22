using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Viam.Common.V1;
using Viam.Component.Board.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Board(ResourceName resourceName, ViamChannel channel) : ComponentBase<Board, BoardService.BoardServiceClient>(resourceName, new BoardService.BoardServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Board(name, channel), () => null));
        public static SubType SubType = SubType.FromRdkComponent("board");

        public static Board FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Board>(resourceName);
        }

        public override async ValueTask<IDictionary<string, object?>> DoCommandAsync(IDictionary<string, object> command,
            TimeSpan? timeout = null)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
            {
                Name = Name,
                Command = command.ToStruct()
            });

            return res.Result.ToDictionary();
        }

        public ValueTask<AnalogReader> GetAnalogReaderByName(string name) => new(new AnalogReader(name, this));
        public ValueTask<AnalogWriter> GetAnalogWriterByName(string name) => new(new AnalogWriter(name, this));
        public ValueTask<DigitalInterrupt> GetDigitalInterruptByName(string name) =>
            new(new DigitalInterrupt(name, this));
        public ValueTask<GpioPin> GetGpioPinByName(string name) => new(new GpioPin(name, this));

        public async ValueTask<string[]> GetAnalogReadersByNameAsync(TimeSpan? timeout = null,
                                                                     CancellationToken cancellationToken = default)
        {
            var res = await GetBoardStatusAsync(timeout, cancellationToken).ConfigureAwait(false);
            return res.Analogs.Keys.ToArray();
        }

        public async ValueTask<string[]> GetDigitalInterruptsByNameAsync(TimeSpan? timeout = null,
                                                                         CancellationToken cancellationToken = default)
        {
            var res = await GetBoardStatusAsync(timeout, cancellationToken).ConfigureAwait(false);
            return res.DigitalInterrupts.Keys.ToArray();
        }

        public async ValueTask<BoardStatus> GetBoardStatusAsync(TimeSpan? timeout = null,
                                                                CancellationToken cancellationToken = default)
        {
            var res = await Client.StatusAsync(new StatusRequest() { Name = Name },
                                               deadline: timeout.ToDeadline(),
                                               cancellationToken: cancellationToken);

            return res.Status;
        }

        public async ValueTask SetPowerModeAsync(PowerMode mode,
                                                 TimeSpan duration,
                                                 Struct? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default)
        {
            await Client.SetPowerModeAsync(new SetPowerModeRequest()
                                           {
                                               Name = Name,
                                               PowerMode = mode,
                                               Duration = Duration.FromTimeSpan(duration),
                                               Extra = extra
                                           },
                                           deadline: timeout.ToDeadline(),
                                           cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask WriteAnalogAsync(string pin,
                                                int value,
                                                Struct? extra = null,
                                                TimeSpan? timeout = null,
                                                CancellationToken cancellationToken = default)
        {
            await Client
                  .WriteAnalogAsync(new WriteAnalogRequest()
                                    {
                                        Name = Name, Pin = pin, Value = value, Extra = extra
                                    },
                                    deadline: timeout.ToDeadline(),
                                    cancellationToken: cancellationToken)
                  .ConfigureAwait(false);
        }
    }

    public record AnalogReader(string name, Board board)
    {
        public async ValueTask<int> ReadAsync(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await board.Client.ReadAnalogReaderAsync(
                                     new ReadAnalogReaderRequest()
                                     {
                                         AnalogReaderName = name,
                                         BoardName = board.Name,
                                         Extra = extra
                                     },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);
            return res.Value;
        }
    }

    public record AnalogWriter(string name, Board board)
    {
        public async ValueTask WriteAsync(int value,
                                          Struct? extra = null,
                                          TimeSpan? timeout = null,
                                          CancellationToken cancellationToken = default)
        {
            await board.WriteAnalogAsync(name, value, extra, timeout, cancellationToken)
                       .ConfigureAwait(false);
        }
    }

    public record DigitalInterrupt(string name, Board board)
    {
        public async ValueTask<long> ValueAsync(Struct? extra = null,
                                                TimeSpan? timeout = null,
                                                CancellationToken cancellationToken = default)
        {
            var res = await board.Client.GetDigitalInterruptValueAsync(
                                     new GetDigitalInterruptValueRequest()
                                     {
                                         BoardName = board.Name, DigitalInterruptName = name, Extra = extra
                                     },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);

            return res.Value;
        }
    }

    public record GpioPin(string name, Board board)
    {
        public async ValueTask<bool> GetAsync(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await board.Client.GetGPIOAsync(new GetGPIORequest()
                                                      {
                                                          Name = board.Name, Pin = name, Extra = extra
                                                      },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);

            return res.High;
        }

        public async ValueTask SetAsync(bool value,
                                        Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default)
        {
            await board.Client.SetGPIOAsync(
                           new SetGPIORequest() { Name = board.Name, Pin = name, High = value, Extra = extra },
                           deadline: timeout.ToDeadline(),
                           cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
        }

        public async ValueTask<double> GetPwmAsync(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await board.Client.PWMAsync(
                                     new PWMRequest() { Name = board.Name, Pin = name, Extra = extra },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);

            return res.DutyCyclePct;
        }

        public async ValueTask SetPwmAsync(double dutyCyclePct,
                                           Struct? extra = null,
                                           TimeSpan? timeout = null,
                                           CancellationToken cancellationToken = default)
        {
            await board.Client.SetPWMAsync(new SetPWMRequest()
                                           {
                                               Name = board.Name,
                                               Pin = name,
                                               DutyCyclePct = dutyCyclePct,
                                               Extra = extra
                                           },
                                           deadline: timeout.ToDeadline(),
                                           cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
        }

        public async ValueTask<ulong> GetPwmFrequencyAsync(Struct? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default)
        {
            var res = await board.Client.PWMFrequencyAsync(
                                     new PWMFrequencyRequest()
                                     {
                                         Name = board.Name, Pin = name, Extra = extra
                                     },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);

            return res.FrequencyHz;
        }

        public async ValueTask SetPwmFrequencyAsync(ulong frequencyHz,
                                                    Struct? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default)
        {
            await board.Client.SetPWMFrequencyAsync(new SetPWMFrequencyRequest()
                                                    {
                                                        Name = board.Name,
                                                        Pin = name,
                                                        FrequencyHz = frequencyHz,
                                                        Extra = extra
                                                    },
                                                    deadline: timeout.ToDeadline(),
                                                    cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
        }
    }
}
