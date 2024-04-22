using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Viam.Common.V1;
using Viam.Net.Sdk.Core.Clients;
using Viam.Net.Sdk.Core.Resources.Components;

namespace Viam.Net.Sdk.Core.Resources
{
    public class ResourceManager(ILogger logger)
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

        private readonly ConcurrentDictionary<ResourceName, ResourceBase> _resources = new();

        public void Register(ResourceName resourceName, ResourceBase resource)
        {
            _resources.TryAdd(resourceName, resource);
        }

        public ResourceBase GetResource(ResourceName resourceName)
        {
            if (_resources.TryGetValue(resourceName, out var resource))
            {
                return resource;
            }

            throw new ResourceNotFoundException();
        }

        public void RemoveResource(ResourceName resourceName)
        {
            if (_resources.TryRemove(resourceName, out var _))
                return;
            else
                throw new ResourceNotFoundException();
        }

        public async Task RefreshAsync(RobotClient client)
        {
            var resourceNames = await client.ResourceNamesAsync().ConfigureAwait(false);
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

        private void CreateOrResetClient(ResourceName resourceName, ViamChannel channel)
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
            catch(ResourceRegistrationNotFoundException)
            {
                logger.LogWarning($"Resource {resourceName} not found in registry");
            }
        }
    }

    public class ResourceNotFoundException : Exception;
}
