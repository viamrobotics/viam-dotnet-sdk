using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Arm.V1;

namespace Viam.Core.Resources.Components.Arm
{
    public interface IArm : IComponentBase
    {
        ValueTask<Pose> GetEndPosition(Struct? extra = null,
                                       TimeSpan? timeout = null,
                                       CancellationToken cancellationToken = default);

        ValueTask MoveToPosition(Pose pose,
                                 Struct? extra = null,
                                 TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask MoveToJoinPositions(JointPositions jointPositions,
                                      Struct? extra = null,
                                      TimeSpan? timeout = null,
                                      CancellationToken cancellationToken = default);

        ValueTask<JointPositions> GetJointPositions(Struct? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default);

        ValueTask Stop(Struct? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask<(KinematicsFileFormat, ByteString)> GetKinematics(Struct? extra = null,
                                                                    TimeSpan? timeout = null,
                                                                    CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(Struct? extra = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}
