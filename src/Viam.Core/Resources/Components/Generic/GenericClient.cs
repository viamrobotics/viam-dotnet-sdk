using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Viam.Common.V1;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;
using Viam.Contracts;
using Viam.Contracts.Resources;

namespace Viam.Core.Resources.Components.Generic
{
    public class GenericClient(ViamResourceName resourceName, ViamChannel channel, ILogger<GenericClient> logger) :
        ComponentBase<GenericClient, Component.Generic.V1.GenericService.GenericServiceClient>(resourceName,
            new Component.Generic.V1.GenericService.GenericServiceClient(channel), logger),
        IGenericClient, IComponentClient<IGenericClient>
    {

        public static SubType SubType = SubType.FromRdkComponent("generic");

        public static async Task<IGenericClient> FromMachine(IMachineClient client, string name, TimeSpan? timeout = null, CancellationToken token = default)
        {
            var resourceName = new ViamResourceName(SubType, name);
            return await client.GetComponent<IGenericClient>(resourceName, timeout, token);
        }

        public static IGenericClient FromDependencies(Dependencies dependencies, string name)
        {
            var resourceName = new ViamResourceName(SubType, name);
            if (!dependencies.TryGetValue(resourceName, out var resource))
            {
                throw new ArgumentException($"Dependency {resourceName} not found");
            }
            if (resource is not IGenericClient client)
            {
                throw new ArgumentException($"Dependency {resourceName} is not a {nameof(IGenericClient)}");
            }
            return client;
        }

        public override DateTime? LastReconfigured { get; }

        public override async ValueTask<Struct?> DoCommand(
            Struct command,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Logger.LogMethodInvocationStart(parameters: [Name, command]);
            try
            {
                var res = await Client
                    .DoCommandAsync(
                        new DoCommandRequest() { Name = ResourceName.Name, Command = command },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (res is null)
                {
                    Logger.LogMethodInvocationSuccess(results: null);
                    return null;
                }

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

        public override ValueTask StopResource() => throw new NotImplementedException();


        public async ValueTask<Geometry[]> GetGeometries(Struct? extra = null,
            TimeSpan? timeout = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            Logger.LogMethodInvocationStart(parameters: [Name]);
            try
            {
                var res = await Client.GetGeometriesAsync(
                        new GetGeometriesRequest() { Name = ResourceName.Name, Extra = extra },
                        deadline: timeout.ToDeadline(),
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                var geometry = res.Geometries.ToArray();
                Logger.LogMethodInvocationSuccess(results: geometry);
                return geometry;
            }
            catch (Exception ex)
            {
                Logger.LogMethodInvocationFailure(ex);
                throw;
            }
        }
    }
}