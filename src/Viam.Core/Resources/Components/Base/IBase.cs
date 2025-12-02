using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Contracts;

namespace Viam.Core.Resources.Components.Base
{
    public interface IBase : IComponentBase
    {
        /// <summary>
        /// Move straight
        /// </summary>
        /// <param name="distance">The distance to move in mm</param>
        /// <param name="velocity">The velocity to move at in mm/sec</param>
        /// <param name="extra"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask MoveStraight(long distance,
            double velocity,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Spin the base
        /// </summary>
        /// <param name="angle">The angle to rotate in degrees</param>
        /// <param name="velocity">The velocity to spin in degrees/sec</param>
        /// <param name="extra"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask Spin(double angle,
            double velocity,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Set the power of all motors on the base
        /// </summary>
        /// <param name="linear">The linear <see cref="Vector3"/></param>
        /// <param name="angular">The angular <see cref="Vector3"/></param>
        /// <param name="extra"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask SetPower(Vector3 linear,
            Vector3 angular,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask SetVelocity(Vector3 linear,
            Vector3 angular,
            Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<BaseProperties> GetProperties(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default);
    }

    public interface IBaseClient : IBase;
}