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
    public class Arm(ResourceName resourceName, ViamChannel channel) : ComponentBase<Arm, ArmService.ArmServiceClient>(resourceName, new ArmService.ArmServiceClient(channel)), IArm
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Arm(name, channel), manager => new Services.Arm()));

        public static SubType SubType = SubType.FromRdkComponent("arm");

        public static Arm FromRobot(RobotClientBase client, string name)
        {
            var resourceName = IResourceBase.GetResourceName(SubType, name);
            return client.GetComponent<Arm>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
                                                                                TimeSpan? timeout = null,
                                                                                CancellationToken cancellationToken = default)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
            {
                Name = Name,
                Command = command.ToStruct()
            });

            return res.Result.ToDictionary();
        }

        public async ValueTask<Pose> GetEndPosition(Struct? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .GetEndPositionAsync(
                                new GetEndPositionRequest() { Name = Name, Extra = extra },
                                deadline: timeout.ToDeadline(),
                                cancellationToken: cancellationToken)
                            ;

            return res.Pose;
        }

        public async ValueTask MoveToPosition(Pose pose,
                                              Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            await Client
                  .MoveToPositionAsync(new MoveToPositionRequest()
                  {
                      Name = Name,
                      To = pose,
                      Extra = extra
                  },
                                       deadline: timeout.ToDeadline(),
                                       cancellationToken: cancellationToken)
                  ;
        }

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
                  }, deadline: timeout.ToDeadline(),
                                             cancellationToken: cancellationToken)
                  ;
        }

        public async ValueTask<JointPositions> GetJointPositions(Struct? extra = null,
                                                                 TimeSpan? timeout = null,
                                                                 CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .GetJointPositionsAsync(
                                new GetJointPositionsRequest() { Name = Name, Extra = extra },
                                deadline: timeout.ToDeadline(),
                                cancellationToken: cancellationToken)
                            ;

            return res.Positions;
        }

        public async ValueTask Stop(Struct? extra = null, TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        ;
        }

        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken)
                                  ;

            return res.IsMoving;
        }

        public async ValueTask<(KinematicsFileFormat, ByteString)> GetKinematics(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            var res = await Client
                            .GetKinematicsAsync(new GetKinematicsRequest() { Name = Name, Extra = extra },
                                                deadline: timeout.ToDeadline(),
                                                cancellationToken: cancellationToken)
                            ;

            return (res.Format, res.KinematicsData);
        }

        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var res = await Client.GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra }, deadline: timeout.ToDeadline(), cancellationToken: cancellationToken);
            return res.Geometries.ToArray();
        }
    }
}
