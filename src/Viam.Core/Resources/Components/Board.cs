using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Board.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components
{
    public interface IBoard : IComponentBase
    {
        /// <summary>
        /// Get a <see cref="AnalogReader"/> from the <see cref="Board"/>
        /// </summary>
        /// <param name="name">The name of the pin capable of reading analog signals on the <see cref="Board"/></param>
        /// <returns>A <see cref="AnalogReader"/></returns>
        ValueTask<AnalogReader> GetAnalogReaderByName(string name);

        /// <summary>
        /// Get a <see cref="AnalogWriter"/> from the <see cref="Board"/>
        /// </summary>
        /// <param name="name">The name of the pin capable of outputting analog signals on the <see cref="Board"/></param>
        /// <returns>A <see cref="AnalogWriter"/></returns>
        ValueTask<AnalogWriter> GetAnalogWriterByName(string name);

        /// <summary>
        /// Get a <see cref="DigitalInterrupt"/> from the <see cref="Board"/>
        /// </summary>
        /// <param name="name">The name of the pin capable of handling interrupts on the <see cref="Board"/></param>
        /// <returns>A <see cref="DigitalInterrupt"/></returns>
        ValueTask<DigitalInterrupt> GetDigitalInterruptByName(string name);

        /// <summary>
        /// Get a <see cref="GpioPin"/> from the <see cref="Board"/>
        /// </summary>
        /// <param name="name">The name of the pin</param>
        /// <returns>A <see cref="GpioPin"/></returns>
        ValueTask<GpioPin> GetGpioPinByName(string name);

        /// <summary>
        /// Set the power mode of the <see cref="Board"/>
        /// </summary>
        /// <param name="mode">The <see cref="PowerMode"/> to apply to the <see cref="Board"/></param>
        /// <param name="duration">The length of time the <paramref name="mode"/> should apply for</param>
        /// <param name="extra">Any extras for the command</param>
        /// <param name="timeout">The timeout to apply to the operation</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for the operation</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the request completes</returns>
        ValueTask SetPowerModeAsync(PowerMode mode,
                          TimeSpan duration,
                          Struct? extra = null,
                          TimeSpan? timeout = null,
                          CancellationToken cancellationToken = default);

        /// <summary>
        /// Write an analog value to an analog capable pin on the <see cref="Board"/>
        /// </summary>
        /// <param name="pin">The name of the pin to write the <paramref name="value"/> to</param>
        /// <param name="value">The value to write to the <paramref name="pin"/></param>
        /// <param name="extra">Any extras for the command</param>
        /// <param name="timeout">The timeout to apply to the operation</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to use for the operation</param>
        /// <returns>A <see cref="ValueTask"/> that completes when the request completes</returns>
        ValueTask WriteAnalogAsync(string pin,
                                   int value,
                                   Struct? extra = null,
                                   TimeSpan? timeout = null,
                                   CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// A <see cref="Board"/> client capable of interacting with a <see cref="Board"/> component on a machine
    /// </summary>
    /// <param name="resourceName">The <see cref="ResourceName"/> of the component</param>
    /// <param name="channel">The <see cref="ViamChannel"/> to use for communication with the component</param>
    /// <param name="logger">A logger</param>
    public class Board(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<Board, BoardService.BoardServiceClient>(resourceName,
                                                                new BoardService.BoardServiceClient(channel)), IBoard
    {
        internal static void RegisterType() => Registry.RegisterSubtype(
            new ResourceRegistration(SubType,
                                     (name, channel, logger) => new Board(name, channel, logger),
                                     (logger) => new Services.Board(logger)));

        public static SubType SubType = SubType.FromRdkComponent("board");

        /// <summary>
        /// Get a <see cref="Board"/> by <paramref name="name"/> from the supplied <see cref="RobotClientBase"/>
        /// </summary>
        /// <param name="client">The <see cref="RobotClientBase"/></param>
        /// <param name="name">The name of the component</param>
        /// <returns>A <see cref="Board"/> component</returns>
        [LogCall]
        public static Board FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Board>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        [LogCall]
        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Result.ToDictionary();
        }

        [LogCall]
        public ValueTask<AnalogReader> GetAnalogReaderByName(string name) => new(new AnalogReader(name, this));

        [LogCall]
        public ValueTask<AnalogWriter> GetAnalogWriterByName(string name) => new(new AnalogWriter(name, this));

        [LogCall]
        public ValueTask<DigitalInterrupt> GetDigitalInterruptByName(string name) =>
            new(new DigitalInterrupt(name, this));

        [LogCall]
        public ValueTask<GpioPin> GetGpioPinByName(string name) => new(new GpioPin(name, this));

        [LogCall]
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

        [LogCall]
        public async ValueTask WriteAnalogAsync(string pin,
                                                int value,
                                                Struct? extra = null,
                                                TimeSpan? timeout = null,
                                                CancellationToken cancellationToken = default)
        {
            await Client
                  .WriteAnalogAsync(new WriteAnalogRequest() { Name = Name, Pin = pin, Value = value, Extra = extra },
                                    deadline: timeout.ToDeadline(),
                                    cancellationToken: cancellationToken)
                  .ConfigureAwait(false);
        }
    }

    public record AnalogReader(string name, Board board)
    {
        [LogCall]
        public async ValueTask<int> ReadAsync(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await board.Client.ReadAnalogReaderAsync(
                                     new ReadAnalogReaderRequest()
                                     {
                                         AnalogReaderName = name, BoardName = board.Name, Extra = extra
                                     },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);
            return res.Value;
        }
    }

    public record AnalogWriter(string name, Board board)
    {
        [LogCall]
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
        [LogCall]
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
        [LogCall]
        public async ValueTask<bool> GetAsync(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await board.Client.GetGPIOAsync(
                                     new GetGPIORequest() { Name = board.Name, Pin = name, Extra = extra },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);

            return res.High;
        }

        [LogCall]
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

        [LogCall]
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

        [LogCall]
        public async ValueTask SetPwmAsync(double dutyCyclePct,
                                           Struct? extra = null,
                                           TimeSpan? timeout = null,
                                           CancellationToken cancellationToken = default)
        {
            await board.Client.SetPWMAsync(new SetPWMRequest()
                                           {
                                               Name = board.Name, Pin = name, DutyCyclePct = dutyCyclePct, Extra = extra
                                           },
                                           deadline: timeout.ToDeadline(),
                                           cancellationToken: cancellationToken)
                       .ConfigureAwait(false);
        }

        [LogCall]
        public async ValueTask<ulong> GetPwmFrequencyAsync(Struct? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default)
        {
            var res = await board.Client.PWMFrequencyAsync(
                                     new PWMFrequencyRequest() { Name = board.Name, Pin = name, Extra = extra },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                                 .ConfigureAwait(false);

            return res.FrequencyHz;
        }

        [LogCall]
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
