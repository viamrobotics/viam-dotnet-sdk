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

namespace Viam.Core.Resources.Components.Board
{
    /// <summary>
    /// A <see cref="Board"/> client capable of interacting with a <see cref="Board"/> component on a machine
    /// </summary>
    /// <param name="resourceName">The <see cref="ResourceName"/> of the component</param>
    /// <param name="channel">The <see cref="ViamChannel"/> to use for communication with the component</param>
    /// <param name="logger">A logger</param>
    public class Board(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<Board, Component.Board.V1.BoardService.BoardServiceClient>(resourceName, new Component.Board.V1.BoardService.BoardServiceClient(channel)),
          IBoard
    {
        static Board() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new Board(name, channel, logger)));

        public static SubType SubType = SubType.FromRdkComponent("board");

        /// <summary>
        /// Get a <see cref="Board"/> by <paramref name="name"/> from the supplied <see cref="RobotClientBase"/>
        /// </summary>
        /// <param name="client">The <see cref="RobotClientBase"/></param>
        /// <param name="name">The name of the component</param>
        /// <returns>A <see cref="Board"/> component</returns>

        public static Board FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Board>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => ValueTask.CompletedTask;

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                var response = res.Result.ToDictionary();
                logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public ValueTask<AnalogReader> GetAnalogReaderByName(string name) => new(new AnalogReader(logger, name, this));


        public ValueTask<AnalogWriter> GetAnalogWriterByName(string name) => new(new AnalogWriter(logger, name, this));


        public ValueTask<DigitalInterrupt> GetDigitalInterruptByName(string name) =>
            new(new DigitalInterrupt(logger, name, this));


        public ValueTask<GpioPin> GetGpioPinByName(string name) => new(new GpioPin(logger, name, this));


        public async ValueTask SetPowerModeAsync(PowerMode mode,
                                                 TimeSpan duration,
                                                 Struct? extra = null,
                                                 TimeSpan? timeout = null,
                                                 CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters:[Name, mode, Duration.FromTimeSpan(duration)]);
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
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask WriteAnalogAsync(string pin,
                                                int value,
                                                Struct? extra = null,
                                                TimeSpan? timeout = null,
                                                CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters:[Name, pin, value]);
                await Client
                      .WriteAnalogAsync(new WriteAnalogRequest() { Name = Name, Pin = pin, Value = value, Extra = extra },
                                        deadline: timeout.ToDeadline(),
                                        cancellationToken: cancellationToken)
                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }

    public record AnalogReader(ILogger logger, string name, Board board)
    {

        public async ValueTask<int> ReadAsync(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [board.Name, name]);
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
                logger.LogMethodInvocationSuccess(results: res.Value);
                return res.Value;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }

    public record AnalogWriter(ILogger logger, string name, Board board)
    {
        public async ValueTask WriteAsync(int value,
                                          Struct? extra = null,
                                          TimeSpan? timeout = null,
                                          CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [board.Name, name, value]);
                await board.WriteAnalogAsync(name, value, extra, timeout, cancellationToken)
                           .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }

    public record DigitalInterrupt(ILogger logger, string name, Board board)
    {

        public async ValueTask<long> ValueAsync(Struct? extra = null,
                                                TimeSpan? timeout = null,
                                                CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [board.Name, name]);
                var res = await board.Client.GetDigitalInterruptValueAsync(
                                         new GetDigitalInterruptValueRequest()
                                         {
                                             BoardName = board.Name,
                                             DigitalInterruptName = name,
                                             Extra = extra
                                         },
                                         deadline: timeout.ToDeadline(),
                                         cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.Value);
                return res.Value;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }

    public record GpioPin(ILogger logger, string name, Board board)
    {

        public async ValueTask<bool> GetAsync(Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters:[board.Name, name]);
                var res = await board.Client.GetGPIOAsync(
                                         new GetGPIORequest() { Name = board.Name, Pin = name, Extra = extra },
                                         deadline: timeout.ToDeadline(),
                                         cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.High);
                return res.High;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask SetAsync(bool value,
                                        Struct? extra = null,
                                        TimeSpan? timeout = null,
                                        CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters:[board.Name, name, value]);
                await board.Client.SetGPIOAsync(
                               new SetGPIORequest() { Name = board.Name, Pin = name, High = value, Extra = extra },
                               deadline: timeout.ToDeadline(),
                               cancellationToken: cancellationToken)
                           .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<double> GetPwmAsync(Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters:[board.Name, name]);
                var res = await board.Client.PWMAsync(
                                         new PWMRequest() { Name = board.Name, Pin = name, Extra = extra },
                                         deadline: timeout.ToDeadline(),
                                         cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.DutyCyclePct);
                return res.DutyCyclePct;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask SetPwmAsync(double dutyCyclePct,
                                           Struct? extra = null,
                                           TimeSpan? timeout = null,
                                           CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters:[board.Name, name, dutyCyclePct]);
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
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<ulong> GetPwmFrequencyAsync(Struct? extra = null,
                                                           TimeSpan? timeout = null,
                                                           CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [board.Name, name]);
                var res = await board.Client.PWMFrequencyAsync(
                                         new PWMFrequencyRequest() { Name = board.Name, Pin = name, Extra = extra },
                                         deadline: timeout.ToDeadline(),
                                         cancellationToken: cancellationToken)
                                     .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.FrequencyHz);
                return res.FrequencyHz;

            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask SetPwmFrequencyAsync(ulong frequencyHz,
                                                    Struct? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters:[board.Name, name, frequencyHz]);
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
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}
