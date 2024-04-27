using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Viam.Common.V1;
using Viam.Core.Clients;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;

namespace Viam.Core.Resources
{
    public class ResourceManager(ILogger<ResourceManager> logger) : IAsyncDisposable
    {
        // This is needed to make sure all well-known types get registered at startup
        static ResourceManager()
        {
            Arm.RegisterType();
            Base.RegisterType();
            Board.RegisterType();
            Camera.RegisterType();
            Encoder.RegisterType();
            Gantry.RegisterType();
            Gripper.RegisterType();
            InputController.RegisterType();
            Motor.RegisterType();
            MovementSensor.RegisterType();
            PowerSensor.RegisterType();
            Sensor.RegisterType();
            Servo.RegisterType();
        }

        private readonly ConcurrentDictionary<ViamResourceName, IResourceBase> _resources = new();

        public void Register(ViamResourceName resourceName, IResourceBase resource)
        {
            logger.LogDebug("Registering resource {ResourceName}", resourceName);
            _resources.TryAdd(resourceName, resource);
        }

        public IResourceBase GetResource(ViamResourceName resourceName)
        {
            if (_resources.TryGetValue(resourceName, out var resource))
            {
                return resource;
            }

            throw new ResourceNotFoundException(resourceName.ToString());
        }

        // This is a little inefficient, but we can fix this later
        public IResourceBase GetResourceByShortName(string name)
        {
            foreach (var item in _resources.Values)
            {
                if (item.ResourceName.Name == name)
                {
                    return item;
                }
            }

            throw new ResourceNotFoundException(name);
        }

        public void RemoveResource(ViamResourceName resourceName)
        {
            if (_resources.TryRemove(resourceName, out var _))
                return;
            else
                throw new ResourceNotFoundException(resourceName.ToString());
        }

        public async Task RefreshAsync(RobotClientBase client)
        {
            var resourceNames = await client.ResourceNamesAsync();
            var resourceNamesEnumerable = resourceNames.ToArray();
            var foo = resourceNamesEnumerable.Where(x => x.Type == "component" || x.Type == "service")
                                             .Where(x => x.Subtype != "remote");
            // TODO: Filter out movement sensors
            //.Where(x => x.Subtype != Sensor.SubType.ResourceSubType && !resourceNamesEnumerable.Contains(MovementSensor.GetResourceName(x.Name)));
            foreach (var fo in foo)
            {
                CreateOrResetClient(fo, client.Channel);
            }
        }

        public ICollection<ViamResourceName> GetResourceNames() => _resources.Keys;

        public ICollection<IResourceBase> GetResources() => _resources.Values;

        private void CreateOrResetClient(ViamResourceName resourceName, ViamChannel channel)
        {
            if (_resources.TryGetValue(resourceName, out var resource))
            {
                // TODO: Implement reconfigurable
                RemoveResource(resourceName);
            }

            try
            {
                var client = Registry.LookupSubtype(SubType.FromResourceName(resourceName))
                                     .CreateRpcClient(resourceName, channel);
                Register(resourceName, client);
            }
            catch (ResourceRegistrationNotFoundException)
            {
                logger.LogWarning($"Resource {resourceName} not found in registry");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _resources.Values.Select(x => x.DisposeAsync())
                            .WhenAll();
        }
    }

    public class ResourceNotFoundException(string name) : Exception($"No resource with name {name} found");
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
