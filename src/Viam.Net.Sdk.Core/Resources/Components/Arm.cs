using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Arm.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Utils;

namespace Viam.Net.Sdk.Core.Resources.Components
{
    public class Arm(ResourceName resourceName, ViamChannel channel) : ComponentBase<Arm, ArmService.ArmServiceClient>(resourceName, new ArmService.ArmServiceClient(channel))
    {
        internal static void RegisterType() => Registry.RegisterSubtype(new ResourceRegistration(SubType, (name, channel) => new Arm(name, channel), () => null));

        public static SubType SubType = SubType.FromRdkComponent("arm");

        public static Arm FromRobot(RobotClient client, string name)
        {
            var resourceName = GetResourceName(SubType, name);
            return client.GetComponent<Arm>(resourceName);
        }

        public override async ValueTask<IDictionary<string, object?>> DoCommandAsync(IDictionary<string, object> command,
            TimeSpan? timeout = null)
        {
            var res = await Client.DoCommandAsync(new DoCommandRequest()
            {
                Name = Name,
                Command = command.ToStruct()
            });

            return res.Result.ToDictionary();
        }

        public async ValueTask<Pose> GetEndPositionAsync(Struct? extra = null,
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

        public async ValueTask MoveToPositionAsync(Pose pose,
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
                  .ConfigureAwait(false);
        }

        public async ValueTask MoveToJoinPositionsAsync(JointPositions jointPositions,
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
                  .ConfigureAwait(false);
        }

        public async ValueTask<JointPositions> GetJointPositionsAsync(Struct? extra = null,
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

        public async ValueTask StopAsync(Struct? extra = null, TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
                                   deadline: timeout.ToDeadline(),
                                   cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
        }

        public async ValueTask<bool> IsMovingAsync(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                 deadline: timeout.ToDeadline(),
                                                 cancellationToken: cancellationToken)
                                  .ConfigureAwait(false);

            return res.IsMoving;
        }

        public async ValueTask<(KinematicsFileFormat, ByteString)> GetKinematicsAsync(Struct? extra = null,
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
    }
}
