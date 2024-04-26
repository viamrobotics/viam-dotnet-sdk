using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Viam.Common.V1;
using Viam.Component.Board.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IBoard : IComponentBase
    {
        ValueTask<AnalogReader> GetAnalogReaderByName(string name);
        ValueTask<AnalogWriter> GetAnalogWriterByName(string name);
        ValueTask<DigitalInterrupt> GetDigitalInterruptByName(string name);
        ValueTask<GpioPin> GetGpioPinByName(string name);

        ValueTask SetPowerModeAsync(PowerMode mode,
                          TimeSpan duration,
                          Struct? extra = null,
                          TimeSpan? timeout = null,
                          CancellationToken cancellationToken = default);

        ValueTask WriteAnalogAsync(string pin,
                                   int value,
                                   Struct? extra = null,
                                   TimeSpan? timeout = null,
                                   CancellationToken cancellationToken = default);
    }

    public class Board(ResourceName resourceName, ViamChannel channel) : ComponentBase<Board, BoardService.BoardServiceClient>(resourceName, new BoardService.BoardServiceClient(channel)), IBoard
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Board(name, channel), manager => new Services.Board()));
        public static SubType SubType = SubType.FromRdkComponent("board");

        public static Board FromRobot(RobotClientBase client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Board>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest() { Name = Name, Command = command.ToStruct() });

            return res.Result.ToDictionary();
        }

        public ValueTask<AnalogReader> GetAnalogReaderByName(string name) => new(new AnalogReader(name, this));
        public ValueTask<AnalogWriter> GetAnalogWriterByName(string name) => new(new AnalogWriter(name, this));
        public ValueTask<DigitalInterrupt> GetDigitalInterruptByName(string name) =>
            new(new DigitalInterrupt(name, this));
        public ValueTask<GpioPin> GetGpioPinByName(string name) => new(new GpioPin(name, this));

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
                        ;
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
                  ;
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
                                 ;
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
                       ;
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
                                 ;

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
                                 ;

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
                       ;
        }

        public async ValueTask<double> GetPwmAsync(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            var res = await board.Client.PWMAsync(
                                     new PWMRequest() { Name = board.Name, Pin = name, Extra = extra },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 ;

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
                       ;
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
                                 ;

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
                       ;
        }
    }
}
