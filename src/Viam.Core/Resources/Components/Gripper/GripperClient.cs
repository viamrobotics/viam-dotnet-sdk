using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Component.Gripper.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;
using Viam.Contracts;
using Viam.Contracts.Resources;

namespace Viam.Core.Resources.Components.Gripper
{
    public class GripperClient(ViamResourceName resourceName, ViamChannel channel, ILogger<GripperClient> logger) :
        ComponentBase<GripperClient, Component.Gripper.V1.GripperService.GripperServiceClient>(resourceName,
            new Component.Gripper.V1.GripperService.GripperServiceClient(channel), logger),
        IGripperClient, IComponentClient<IGripperClient>
    {
        public static SubType SubType = SubType.FromRdkComponent("gripper");

        public static async Task<IGripperClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<IGripperClient>(resourceName, timeout, token);
        }

        public static IGripperClient FromDependencies(Dependencies dependencies, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            if (!dependencies.TryGetValue(resourceName, out var resource))
            {
                throw new ArgumentException($"Dependency {resourceName} not found");
            }
            if (resource is not IGripperClient client)
            {
                throw new ArgumentException($"Dependency {resourceName} is not a {nameof(IGripperClient)}");
            }
            return client;
        }

        public override DateTime? LastReconfigured => null;

        public override ValueTask StopResource() => Stop();

        public override async ValueTask<Struct> DoCommand(Struct command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name, command]);
                var res = await Client.DoCommandAsync(new DoCommandRequest()
                {
                    Name = ResourceName.Name,
                    Command = command
                },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var response = res.Result;
                Logger.LogMethodInvocationSuccess(results: response);
                return response;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }


        public async ValueTask Open(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.OpenAsync(new OpenRequest() { Name = Name, Extra = extra },
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


        public async ValueTask Grab(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.GrabAsync(new GrabRequest() { Name = Name, Extra = extra },
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


        public async ValueTask Stop(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            try
            {
                Logger.LogMethodInvocationStart(parameters: [Name]);
                await Client.StopAsync(new StopRequest() { Name = Name, Extra = extra },
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
    }
}