using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Component.Board.V1;

namespace Viam.Core.Resources.Components.Board
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
}
