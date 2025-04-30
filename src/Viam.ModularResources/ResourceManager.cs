using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Google.Api;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Viam.Core;
using Viam.Core.Clients;
using Viam.Core.Logging;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;
using Viam.Core.Resources.Components.MovementSensor;
using Viam.Core.Resources.Components.Sensor;
using Viam.Core.Utils;

namespace Viam.ModularResources
{
    public class ResourceManager : IAsyncDisposable
    {
        private readonly ILogger<ResourceManager> _logger;
        private readonly IServiceProvider _services;
        private readonly ConcurrentDictionary<string, IModularResource> _resources = new();
        public readonly IDictionary<SubType, ResourceInfo> RegisteredResources = new Dictionary<SubType, ResourceInfo>();
        public ResourceManager(ILogger<ResourceManager> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        public IModularResource GetService(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }
            if (_resources.TryGetValue(name, out var resource))
            {
                _logger.LogTrace("Found service: {ServiceName}", resource.Name);
                return resource;
            }
            throw new ResourceNotFoundException($"Resource {name} not found");
        }

        public void RemoveResource(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }
            if (_resources.TryRemove(name, out var resource))
            {
                if (resource is IDisposable syncDisposable)
                {
                    syncDisposable.Dispose();
                }
                else
                {
                    _logger.LogWarning("Service {ServiceName} not disposed, does not implement IDisposable, how did it get injected to start with?", name);
                }
                _logger.LogInformation("Removed service: {ServiceName}", resource.Name);
            }
            else
            {
                _logger.LogWarning("Failed to remove service: {ServiceName}", name);
            }
        }

        public IModularResource ResolveService(string name, SubType subType)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Name cannot be null or empty", nameof(name));
            }
            return _resources.GetOrAdd(name, (s) =>
            {
                var resource = (IModularResource)ActivatorUtilities.CreateInstance(_services, RegisteredResources[subType].Type, new ViamResourceName(subType, name));
                _logger.LogInformation("Constructed service: {ServiceName}", resource.Name);
                return resource;
            });
        }

        public void RegisterType<TInterface, TImpl>() 
            where TInterface : IComponentBase 
            where TImpl : IModularResourceService
        {
            RegisteredResources.Add(TImpl.SubType, new(TImpl.SubType, TImpl.Model, Viam.Core.Resources.Service.Lookup<TInterface>(), typeof(TImpl)));
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var resource in _resources.Values)
            {
                if (resource is IAsyncDisposable disposable)
                {
                    await disposable.DisposeAsync();
                }
                else if (resource is IDisposable syncDisposable)
                {
                    syncDisposable.Dispose();
                }
            }
        }

        public readonly record struct ResourceInfo(SubType SubType, Model Model, Core.Resources.Service ServiceName, Type Type)
        {
            public override string ToString() => $"{Model} - {SubType} - {ServiceName} - {Type.Name}";
        }
    }
}
