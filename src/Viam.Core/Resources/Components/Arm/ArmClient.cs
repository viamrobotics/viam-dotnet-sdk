using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Viam.Common.V1;
using Viam.Component.Arm.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Arm
{
    public class ArmClient(ViamResourceName resourceName, ViamChannel channel, ILogger<ArmClient> logger)
        : ComponentBase<ArmClient, Component.Arm.V1.ArmService.ArmServiceClient>(resourceName, new Component.Arm.V1.ArmService.ArmServiceClient(channel), logger),
            IArmClient, IComponentClient<IArmClient>
    {
        public static SubType SubType = SubType.FromRdkComponent("arm");

        public static async Task<IArmClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<IArmClient>(resourceName, timeout, token);
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<Dictionary<string, object?>> DoCommand(IDictionary<string, object?> command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(
                        new DoCommandRequest() { Name = Name, Command = command.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var response = res.Result.ToDictionary();
                Logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Pose> GetEndPosition(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                    .GetEndPositionAsync(
                        new GetEndPositionRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess(results: res.Pose);
                return res.Pose;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask MoveToPosition(Pose pose,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, pose]);
                await Client.MoveToPositionAsync(
                        new MoveToPositionRequest() { Name = Name, To = pose, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                Logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask MoveToJoinPositions(JointPositions jointPositions,
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, jointPositions]);
                await Client
                    .MoveToJointPositionsAsync(new MoveToJointPositionsRequest()
                        {
                            Name = Name,
                            Positions = jointPositions,
                            Extra = extra?.ToStruct()
                        },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<JointPositions> GetJointPositions(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                    .GetJointPositionsAsync(
                        new GetJointPositionsRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.Positions);
                return res.Positions;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask Stop(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<bool> IsMoving(TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.IsMovingAsync(new IsMovingRequest() { Name = Name },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.IsMoving);
                return res.IsMoving;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<(KinematicsFileFormat, ByteString)> GetKinematics(
            IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client
                    .GetKinematicsAsync(new GetKinematicsRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: [res.Format, res.KinematicsData]);
                return (res.Format, res.KinematicsData);
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask<Geometry[]> GetGeometries(IDictionary<string, object?>? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                var res = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = Name, Extra = extra?.ToStruct() },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                Logger.LogMethodInvocationSuccess(results: res.Geometries);
                return res.Geometries.ToArray();
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}