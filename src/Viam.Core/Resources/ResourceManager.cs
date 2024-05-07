using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Utils;

namespace Viam.Core.Resources
{
    public class ResourceManager(ILoggerFactory loggerFactory) : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<ViamResourceName, IResourceBase> _resources = new();
        private readonly ILogger<ResourceManager> _logger = loggerFactory.CreateLogger<ResourceManager>();

        [LogInvocation]
        public void Register(ViamResourceName resourceName, IResourceBase resource, [CallerMemberName] string? caller = null)
        {
            _logger.LogManagerResourceRegistration(resourceName, resource, caller);
            if (_resources.TryAdd(resourceName, resource) == false)
            {
                throw new ResourceAlreadyRegisteredException();
            }
        }

        [LogInvocation]
        public IResourceBase GetResource(ViamResourceName resourceName, [CallerMemberName] string? caller = null)
        {
            _logger.LogManagerResourceGet(resourceName, caller);
            if (_resources.TryGetValue(resourceName, out var resource))
            {
                _logger.LogManagerResourceGetSuccess(resourceName, resource, caller);
                return resource;
            }

            _logger.LogManagerResourceGetFailure(resourceName, caller);
            throw new ResourceNotFoundException(resourceName.ToString());
        }

        // This is a little inefficient, but we can fix this later
        [LogInvocation]
        public IResourceBase GetResourceByShortName(string name, [CallerMemberName] string? caller = null)
        {
            _logger.LogManagerResourceGetByShortName(name, caller);
            foreach (var item in _resources.Values)
            {
                if (item.ResourceName.Name == name)
                {
                    _logger.LogManagerResourceGetByShortNameSuccess(name, item, caller);
                    return item;
                }
            }

            _logger.LogManagerResourceGetByShortNameFailure(name, caller);
            throw new ResourceNotFoundException(name);
        }

        [LogInvocation]
        public void RemoveResource(ViamResourceName resourceName, [CallerMemberName] string? caller = null)
        {
            _logger.LogManagerResourceRemove(resourceName, caller);
            if (_resources.TryRemove(resourceName, out _))
            {
                _logger.LogManagerResourceRemoveSuccess(resourceName, caller);
                return;
            }

            _logger.LogManagerResourceRemoveFailure(resourceName, caller);
            throw new ResourceNotFoundException(resourceName.ToString());
        }

        [LogInvocation]
        public async Task RefreshAsync(RobotClientBase client, [CallerMemberName] string? caller = null)
        {
            _logger.LogManagerRefreshStart(caller);
            var resourceNames = await client.ResourceNamesAsync();
            var resourceNamesEnumerable = resourceNames.ToArray();
            var foo = resourceNamesEnumerable.Where(x => x.ResourceType == "component" || x.ResourceType == "service")
                                             .Where(x => x.ResourceSubtype != "remote");
            // TODO: Filter out movement sensors
            // TODO: Filter power sensors?
            //.Where(x => x.Subtype != Sensor.SubType.ResourceSubType && !resourceNamesEnumerable.Contains(MovementSensor.GetResourceName(x.Name)));
            foreach (var fo in foo)
            {
                CreateOrResetClient(fo, client.Channel);
            }

            _logger.LogManagerRefreshFinish(caller);
        }

        [LogInvocation]
        public void RegisterRemoteResources(ViamResourceName[] remoteResourceNames, ViamChannel channel)
        {
            var resourceCount = 0;
            _logger.LogManagerRegisterRemoteResources(remoteResourceNames);
            foreach (var remoteResourceName in remoteResourceNames)
            {
                _logger.LogManagerRegisterRemoteResource(remoteResourceName);
                try
                {
                    var registration = Registry.GetResourceRegistrationBySubtype(SubType.FromResourceName(remoteResourceName));

                    var resource = registration.CreateRpcClient(remoteResourceName,
                                                                channel,
                                                                loggerFactory.CreateLogger(remoteResourceName.Name));

                    Register(remoteResourceName, resource);
                    resourceCount++;
                }
                catch (ResourceException ex)
                {
                    _logger.LogManagerRegisterRemoteResourcesError(remoteResourceName, ex);
                }
            }

            _logger.LogManagerRegisterRemoteResourcesComplete(resourceCount);
        }

        [LogInvocation]
        public ICollection<ViamResourceName> GetResourceNames() => _resources.Keys;

        [LogInvocation]
        public ICollection<IResourceBase> GetResources() => _resources.Values;

        [LogInvocation]
        private void CreateOrResetClient(ViamResourceName resourceName, ViamChannel channel)
        {
            _logger.LogManagerCreateOrRefreshClient(resourceName, channel);
            if (_resources.TryGetValue(resourceName, out var resource))
            {
                // TODO: Implement reconfigurable
                RemoveResource(resourceName);
            }

            try
            {
                var client = Registry.GetResourceRegistrationBySubtype(SubType.FromResourceName(resourceName))
                                     .CreateRpcClient(resourceName, channel, loggerFactory.CreateLogger(resourceName.ResourceSubtype));
                Register(resourceName, client);
            }
            catch (ResourceRegistrationNotFoundException)
            {
                _logger.LogWarning($"Resource {resourceName} not found in registry");
            }
        }

        [LogInvocation]
        public async ValueTask DisposeAsync()
        {
            _logger.LogManagerDispose();
            await _resources.Values.Select(x => x.DisposeAsync())
                            .WhenAll();
            GC.SuppressFinalize(this);
        }
    }

    public abstract class ResourceStatus(ViamResourceName resourceName)
    {
        public readonly ViamResourceName ResourceName = resourceName;
        public abstract DateTime? LastReconfigured { get; protected set; }
        public abstract IDictionary<string, object?> Details { get; }
        public static Func<ResourceBase, ResourceStatus> DefaultCreator => @base => new DefaultResourceStatus(@base.ResourceName, @base.LastReconfigured);
    }

    internal sealed class DefaultResourceStatus(ViamResourceName resourceName, DateTime? lastReconfigured) : ResourceStatus(resourceName)
    {
        public override DateTime? LastReconfigured { get; protected set; } = lastReconfigured;
        public override IDictionary<string, object?> Details { get; } = new Dictionary<string, object?>();
    }
}
