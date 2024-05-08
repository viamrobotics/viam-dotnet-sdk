using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;

namespace Viam.Core.Resources.Components.Motor
{
    public interface IMotor : IResourceBase
    {
        ValueTask SetPower(double power,
                           Struct? extra = null,
                           TimeSpan? timeout = null,
                           CancellationToken cancellationToken = default);

        ValueTask GoFor(double rpm,
                        double revolutions,
                        Struct? extra = null,
                        TimeSpan? timeout = null,
                        CancellationToken cancellationToken = default);

        ValueTask GoTo(double rpm,
                       double positionRevolutions,
                       Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask ResetZeroPosition(double offset,
                                    Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default);

        ValueTask<double> GetPosition(Struct? extra = null,
                                      TimeSpan? timeout = null,
                                      CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<(bool IsOn, double PowerPct)> IsPowered(Struct? extra = null,
                                                          TimeSpan? timeout = null,
                                                          CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask<MotorClient.Properties> GetProperties(Struct? extra = null,
                                                  TimeSpan? timeout = null,
                                                  CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                            TimeSpan? timeout = null,
                                            CancellationToken cancellationToken = default);
    }
}
