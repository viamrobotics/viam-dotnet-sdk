using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Arm.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Arm
{
    public class ArmClient(ViamResourceName resourceName, ViamChannel channel, ILogger logger)
        : ComponentBase<ArmClient, Component.Arm.V1.ArmService.ArmServiceClient>(resourceName, new Component.Arm.V1.ArmService.ArmServiceClient(channel)),
          IArm
    {
        static ArmClient() => Registry.RegisterSubtype(new ComponentRegistration(SubType, (name, channel, logger) => new ArmClient(name, channel, logger)));
        public static SubType SubType = SubType.FromRdkComponent("arm");

        public static ArmClient FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<ArmClient>(resourceName);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<IDictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(
                                          new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
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


        public async ValueTask<Pose> GetEndPosition(Struct? extra = null,
                                                    TimeSpan? timeout = null,
                                                    CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                              .GetEndPositionAsync(
                                  new GetEndPositionRequest() { Name = Name, Extra = extra },
                                  deadline: timeout.ToDeadline(),
                                  cancellationToken: cancellationToken)
                              .ConfigureAwait(false);

                logger.LogMethodInvocationSuccess(results: res.Pose);
                return res.Pose;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask MoveToPosition(Pose pose,
                                              Struct? extra = null,
                                              TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, pose]);
                await Client.MoveToPositionAsync(new MoveToPositionRequest() { Name = Name, To = pose, Extra = extra },
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


        public async ValueTask MoveToJoinPositions(JointPositions jointPositions,
                                                   Struct? extra = null,
                                                   TimeSpan? timeout = null,
                                                   CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name, jointPositions]);
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
                logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<JointPositions> GetJointPositions(Struct? extra = null,
                                                                 TimeSpan? timeout = null,
                                                                 CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                              .GetJointPositionsAsync(
                                  new GetJointPositionsRequest() { Name = Name, Extra = extra },
                                  deadline: timeout.ToDeadline(),
                                  cancellationToken: cancellationToken)
                              .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.Positions);
                return res.Positions;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask Stop(Struct? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
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


        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
                                              CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                                                     deadline: timeout.ToDeadline(),
                                                     cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.IsMoving);
                return res.IsMoving;
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<(KinematicsFileFormat, ByteString)> GetKinematics(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                              .GetKinematicsAsync(new GetKinematicsRequest() { Name = Name, Extra = extra },
                                                  deadline: timeout.ToDeadline(),
                                                  cancellationToken: cancellationToken)
                              .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: [res.Format, res.KinematicsData]);
                return (res.Format, res.KinematicsData);
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
                                                         TimeSpan? timeout = null,
                                                         CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetGeometriesAsync(new GetGeometriesRequest() { Name = Name, Extra = extra },
                                                          deadline: timeout.ToDeadline(),
                                                          cancellationToken: cancellationToken)
                                      .ConfigureAwait(false);
                logger.LogMethodInvocationSuccess(results: res.Geometries);
                return res.Geometries.ToArray();
            }
            catch (Exception ex)
            {
                logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}