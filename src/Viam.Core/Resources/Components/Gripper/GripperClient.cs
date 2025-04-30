using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf.WellKnownTypes;

using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Component.Gripper.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources.Components.Gripper
{
    public class GripperClient(ViamResourceName resourceName, ViamChannel channel, ILogger logger) :
        ComponentBase<GripperClient, Component.Gripper.V1.GripperService.GripperServiceClient>(resourceName, new Component.Gripper.V1.GripperService.GripperServiceClient(channel)),
        IGripper
    {
        public static SubType SubType = SubType.FromRdkComponent("gripper");

        public static IGripper FromRobot(RobotClientBase client, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return client.GetComponent<GripperClient>(resourceName);
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
                var res = await Client.DoCommandAsync(new DoCommandRequest()
                {
                    Name = ResourceName.Name,
                    Command = command.ToStruct()
                },
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


        public async ValueTask Open(IDictionary<string, object?>? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.OpenAsync(new OpenRequest() { Name = Name, Extra = extra?.ToStruct() },
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


        public async ValueTask Grab(IDictionary<string, object?>? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.GrabAsync(new GrabRequest() { Name = Name, Extra = extra?.ToStruct() },
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


        public async ValueTask Stop(IDictionary<string, object?>? extra = null,
                                    TimeSpan? timeout = null,
                                    CancellationToken cancellationToken = default)
        {
            try
            {
                logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra?.ToStruct() },
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
    }
}
