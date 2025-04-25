using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Viam.Common.V1;
using Viam.Component.Arm.V1;

namespace Viam.Core.Resources.Components.Arm
{
    public interface IArm : IComponentBase
    {
        ValueTask<Pose> GetEndPosition(IDictionary<string, object?>? extra = null,
                                       TimeSpan? timeout = null,
                                       CancellationToken cancellationToken = default);

        ValueTask MoveToPosition(Pose pose,
                                 IDictionary<string, object?>? extra = null,
                                 TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask MoveToJoinPositions(JointPositions jointPositions,
                                      IDictionary<string, object?>? extra = null,
                                      TimeSpan? timeout = null,
                                      CancellationToken cancellationToken = default);

        ValueTask<JointPositions> GetJointPositions(IDictionary<string, object?>? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default);

        ValueTask Stop(IDictionary<string, object?>? extra = null,
                       TimeSpan? timeout = null,
                       CancellationToken cancellationToken = default);

        ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                 CancellationToken cancellationToken = default);

        ValueTask<(KinematicsFileFormat, ByteString)> GetKinematics(IDictionary<string, object?>? extra = null,
                                                                    TimeSpan? timeout = null,
                                                                    CancellationToken cancellationToken = default);

        ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}
