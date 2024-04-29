using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources
{
    public class ResourceManager(ILoggerFactory loggerFactory) : IAsyncDisposable
    {
        private readonly ConcurrentDictionary<ViamResourceName, IResourceBase> _resources = new();
        private readonly ILogger<ResourceManager> _logger = loggerFactory.CreateLogger<ResourceManager>();

        [LogCall]
        public void Register(ViamResourceName resourceName, IResourceBase resource, [CallerMemberName] string? caller = null)
        {
            Logging.LogMessages.LogManagerResourceRegistration(_logger, resourceName, resource, caller);
            _resources.TryAdd(resourceName, resource);
        }

        [LogCall]
        public IResourceBase GetResource(ViamResourceName resourceName, [CallerMemberName] string? caller = null)
        {
            Logging.LogMessages.LogManagerResourceGet(_logger, resourceName, caller);
            if (_resources.TryGetValue(resourceName, out var resource))
            {
                Logging.LogMessages.LogManagerResourceGetSuccess(_logger, resourceName, resource, caller);
                return resource;
            }

            Logging.LogMessages.LogManagerResourceGetFailure(_logger, resourceName, caller);
            throw new ResourceNotFoundException(resourceName.ToString());
        }

        // This is a little inefficient, but we can fix this later
        [LogCall]
        public IResourceBase GetResourceByShortName(string name, [CallerMemberName] string? caller = null)
        {
            Logging.LogMessages.LogManagerResourceGetByShortName(_logger, name, caller);
            foreach (var item in _resources.Values)
            {
                if (item.ResourceName.Name == name)
                {
                    Logging.LogMessages.LogManagerResourceGetByShortNameSuccess(_logger, name, item, caller);
                    return item;
                }
            }

            Logging.LogMessages.LogManagerResourceGetByShortNameFailure(_logger, name, caller);
            throw new ResourceNotFoundException(name);
        }

        [LogCall]
        public void RemoveResource(ViamResourceName resourceName, [CallerMemberName] string? caller = null)
        {
            Logging.LogMessages.LogManagerResourceRemove(_logger, resourceName, caller);
            if (_resources.TryRemove(resourceName, out _))
            {
                Logging.LogMessages.LogManagerResourceRemoveSuccess(_logger, resourceName, caller);
                return;
            }

            Logging.LogMessages.LogManagerResourceRemoveFailure(_logger, resourceName, caller);
            throw new ResourceNotFoundException(resourceName.ToString());
        }

        [LogCall]
        public async Task RefreshAsync(RobotClientBase client, [CallerMemberName] string? caller = null)
        {
            Logging.LogMessages.LogManagerRefreshStart(_logger, caller);
            var resourceNames = await client.ResourceNamesAsync();
            var resourceNamesEnumerable = resourceNames.ToArray();
            var foo = resourceNamesEnumerable.Where(x => x.ResourceType == "component" || x.ResourceType == "service")
                                             .Where(x => x.ResourceSubtype != "remote");
            // TODO: Filter out movement sensors
            //.Where(x => x.Subtype != Sensor.SubType.ResourceSubType && !resourceNamesEnumerable.Contains(MovementSensor.GetResourceName(x.Name)));
            foreach (var fo in foo)
            {
                CreateOrResetClient(fo, client.Channel);
            }

            Logging.LogMessages.LogManagerRefreshFinish(_logger, caller);
        }

        [LogCall]
        public ICollection<ViamResourceName> GetResourceNames() => _resources.Keys;

        [LogCall]
        public ICollection<IResourceBase> GetResources() => _resources.Values;

        [LogCall]
        private void CreateOrResetClient(ViamResourceName resourceName, ViamChannel channel)
        {
            Logging.LogMessages.LogManagerCreateOrRefreshClient(_logger, resourceName, channel);
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

        [LogCall]
        public async ValueTask DisposeAsync()
        {
            Logging.LogMessages.LogManagerDispose(_logger);
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
