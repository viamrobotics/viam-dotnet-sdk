using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Arm.V1;
using Viam.Core.Clients;
using Viam.Core.Utils;
using System.Linq;
using Microsoft.Extensions.Logging;
using Viam.Core.Logging;

namespace Viam.Core.Resources.Components
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

    public class Arm(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<Arm, ArmService.ArmServiceClient>(resourceName, new ArmService.ArmServiceClient(channel)),
          IArm
    {
        static Arm() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new Arm(name, channel, logger)));
        public static SubType SubType = SubType.FromRdkComponent("arm");

        [LogInvocation]
        public static Arm FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<Arm>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        [LogInvocation]
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

        [LogInvocation]
        public async ValueTask<Pose> GetEndPosition(Struct? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default)
        {
            var res = await Client
                          .GetEndPositionAsync(
                              new GetEndPositionRequest() { Name = Name, Extra = extra },
                              deadline: timeout.ToDeadline(),
                              cancellationToken: cancellationToken)
                          .ConfigureAwait(false);

            return res.Pose;
        }

        [LogInvocation]
        public async ValueTask MoveToPosition(Pose pose,
                                              Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            await Client
                .MoveToPositionAsync(new MoveToPositionRequest() { Name = Name, To = pose, Extra = extra },
                                     deadline: timeout.ToDeadline(),
                                     cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask MoveToJoinPositions(JointPositions jointPositions,
                                                   Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            await Client
                .MoveToJointPositionsAsync(new MoveToJointPositionsRequest()
                {
                    Name = Name,
                    Positions = jointPositions,
                    Extra = extra
                },
                                           deadline: timeout.ToDeadline(),
                                           cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask<JointPositions> GetJointPositions(Struct? extra = null,
                                                                 TimeSpan? timeout = null,
                                                                 CancellationToken cancellationToken = default)
        {
            var res = await Client
                          .GetJointPositionsAsync(
                              new GetJointPositionsRequest() { Name = Name, Extra = extra },
                              deadline: timeout.ToDeadline(),
                              cancellationToken: cancellationToken)
                          .ConfigureAwait(false);

            return res.Positions;
        }

        [LogInvocation]
        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        [LogInvocation]
        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.IsMoving;
        }

        [LogInvocation]
        public async ValueTask<(KinematicsFileFormat, ByteString)> GetKinematics(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                          .GetKinematicsAsync(new GetKinematicsRequest() { Name = Name, Extra = extra },
                                              deadline: timeout.ToDeadline(),
                                              cancellationToken: cancellationToken)
                          .ConfigureAwait(false);

            return (res.Format, res.KinematicsData);
        }

        [LogInvocation]
        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra },
                                                      deadline: timeout.ToDeadline(),
                                                      cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.Geometries.ToArray();
        }
    }
}