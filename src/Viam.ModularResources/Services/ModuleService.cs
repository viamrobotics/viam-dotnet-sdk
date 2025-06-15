using Grpc.Core;
using System.Data;
using Viam.Core;
using Viam.Core.Clients;
using Viam.Core.Grpc;
using Viam.Core.Resources;
using Viam.Core.Utils;
using Viam.Module.V1;
using Viam.Robot.V1;
using grpcRobotService = Viam.Module.V1.ModuleService;

namespace Viam.ModularResources.Services
{
    public class ModuleService(IServiceProvider services, ILoggerFactory loggerFactory)
        : grpcRobotService.ModuleServiceBase
    {
        private static readonly SemaphoreSlim ParentAddressLock = new(1);
        private static Uri? _parentAddress;
        private readonly ILogger<ModuleService> _logger = loggerFactory.CreateLogger<ModuleService>();

        public override async Task<AddResourceResponse> AddResource(AddResourceRequest request,
            ServerCallContext context)
        {
            _logger.LogDebug("Starting AddResource...");
            var config = request.Config;
            var subType = SubType.FromString(config.Api);
            var model = Model.FromString(config.Model);
            _logger.LogDebug("Adding resource {Name} {SubType} {Model} with dependencies {Dependencies}",
                config.Name,
                subType,
                model,
                string.Join(",", request.Dependencies.Select(x => x)));

            try
            {
                var channel = await DialParent();
                if (channel == null)
                {
                    throw new Exception("Unable to dial parent");
                }

                _logger.LogDebug("Dialed parent, preparing resources...");

                // Try to load the resources from the parent
                var client = new ViamMachineClientBase(loggerFactory, channel);
                var remoteResourceNames = await client.ResourceNamesAsync();
                foreach (var name in remoteResourceNames)
                {
                    _logger.LogDebug("Found remote resource {ResourceName}", name);
                }

                _logger.LogDebug("Want to add resource {ResourceName} {ResourceSubType} {ResourceModel}", config.Name,
                    subType, model);
                var resourceManager = services.GetRequiredService<ResourceManager>();
                var resource = resourceManager.ResolveService(config.Name, subType) ??
                               throw new Exception($"Unable to find resource {config.Name} {subType} {model}");
                _logger.LogDebug("Got resource {Name}", resource.Name);

                var dependencies = request.Dependencies.Select(GrpcExtensions.ToResourceName)
                    .ToDictionary(x => x, x => (IResourceBase)resourceManager.ResolveService(x.Name, x.SubType));
                _logger.LogDebug("Got dependencies {Dependencies}",
                    string.Join(",", dependencies.Select(x => x.ToString())));

                await ReconfigureResource(resource, config, dependencies);
                return new AddResourceResponse();
            }
            catch (ResourceCreatorRegistrationNotFoundException)
            {
                _logger.LogDebug("Failed to find resource creator for {Model}", model);
                throw;
            }
        }

        public override async Task<ReadyResponse> Ready(ReadyRequest request, ServerCallContext context)
        {
            _logger.LogDebug("Calling ready...");
            _logger.LogDebug("Dialing parent...");
            // We need to force the correct URI scheme so we pre-pend file://
            await SetParentAddress(new Uri($"file://{request.ParentAddress}")).ConfigureAwait(false);
            _logger.LogDebug("Set parent address to {ParentAddress}", _parentAddress);

            var serviceNameToModels = new Dictionary<(string, SubType), ICollection<Model>>();
            var resourceManager = services.GetRequiredService<ResourceManager>();

            _logger.LogDebug("Getting modular resources...");
            foreach (var (subType, resource) in resourceManager.RegisteredResources)
            {
                _logger.LogInformation("Found modular service {ServiceName} {SubType}", resource.ServiceName,
                    resource.SubType);
                var models =
                    serviceNameToModels.GetValueOrDefault((resource.ServiceName, resource.SubType), new List<Model>());
                models.Add(resource.Model);
                serviceNameToModels[(resource.ServiceName, resource.SubType)] = models;
            }

            _logger.LogDebug("Preparing handlers...");
            var handlers = new HandlerMap();
            foreach (var ((serviceName, subType), models) in serviceNameToModels)
            {
                _logger.LogTrace("Preparing handler for {Service}", serviceName);
                var rpcSubtype = new ResourceRPCSubtype()
                    { Subtype = new ViamResourceName(subType, string.Empty), ProtoService = serviceName };
                var handler = new HandlerDefinition() { Subtype = rpcSubtype };
                handler.Models.AddRange(models.Select(x => x.ToString()));
                handlers.Handlers.Add(handler);
            }

            var resp = new ReadyResponse
            {
                Ready = true,
                Handlermap = handlers
            };

            _logger.LogDebug("Done preparing ready response: {ReadyResponse}", resp);
            return resp;
        }

        public override async Task<ReconfigureResourceResponse> ReconfigureResource(ReconfigureResourceRequest request,
            ServerCallContext context)
        {
            var config = request.Config;
            var subType = SubType.FromString(config.Api);
            var model = Model.FromString(config.Model);
            _logger.LogDebug("Reconfiguring resource {Name} {SubType} {Model} with dependencies {Dependencies}",
                config.Name,
                subType,
                model,
                string.Join(",", request.Dependencies.Select(x => x)));
            var resourceManager = services.GetRequiredService<ResourceManager>();
            var resource = resourceManager.ResolveService(config.Name, subType) ??
                           throw new Exception($"Unable to find resource {config.Name} {subType} {model}");
            var dependencies = request.Dependencies.Select(GrpcExtensions.ToResourceName)
                .ToDictionary(x => x, x => (IResourceBase)resourceManager.ResolveService(x.Name, x.SubType));

            await ReconfigureResource(resource, config, dependencies);

            _logger.LogDebug("Done reconfiguring {ResourceName}", resource.Name);

            return new ReconfigureResourceResponse();
        }

        public override Task<RemoveResourceResponse> RemoveResource(RemoveResourceRequest request,
            ServerCallContext context)
        {
            var resourceManager = services.GetRequiredService<ResourceManager>();
            resourceManager.RemoveResource(request.Name);
            return Task.FromResult(new RemoveResourceResponse());
        }

        public override Task<ValidateConfigResponse> ValidateConfig(ValidateConfigRequest request,
            ServerCallContext context)
        {
            var config = request.Config;
            var subType = SubType.FromString(config.Api);
            var model = Model.FromString(config.Model);
            var resourceManager = services.GetRequiredService<ResourceManager>();
            var resource = resourceManager.ResolveService(config.Name, subType) ??
                           throw new Exception($"Unable to find resource {config.Name} {subType} {model}");
            var deps = resource.ValidateConfig(config);
            var resp = new ValidateConfigResponse();
            resp.Dependencies.AddRange(deps);
            return Task.FromResult(resp);
        }

        private async ValueTask SetParentAddress(Uri parentAddress)
        {
            await ParentAddressLock.WaitAsync();
            try
            {
                _parentAddress ??= parentAddress;
            }
            finally
            {
                ParentAddressLock.Release();
            }
        }

        private async ValueTask<ViamChannel?> DialParent()
        {
            await ParentAddressLock.WaitAsync();
            try
            {
                if (_parentAddress == null)
                    return null;
                var dialer = new GrpcDialer(loggerFactory.CreateLogger<GrpcDialer>(), loggerFactory);
                var options = new GrpcDialOptions(_parentAddress, true);
                var channel = await dialer.DialDirectAsync(options);
                return channel;
            }
            finally
            {
                ParentAddressLock.Release();
            }
        }

        private async ValueTask ReconfigureResource(IModularResource resource, App.V1.ComponentConfig config,
            Dictionary<ViamResourceName, IResourceBase> dependencies)
        {
            if (resource is IAsyncReconfigurable asyncReconfigurable)
            {
                _logger.LogDebug("Resource {ResourceName} supports reconfiguration", resource.Name);
                try
                {
                    await asyncReconfigurable.Reconfigure(config, dependencies);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to reconfigure resource {ResourceName}", resource.Name);
                    throw;
                }

                _logger.LogDebug("Reconfigured {ReourceName}", resource.Name);
            }
            else if (resource is IReconfigurable reconfigurable)
            {
                _logger.LogDebug("Resource {ResourceName} supports reconfiguration", resource.Name);
                try
                {
                    reconfigurable.Reconfigure(config, dependencies);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to reconfigure resource {ResourceName}", resource.Name);
                    throw;
                }

                _logger.LogDebug("Reconfigured {ReourceName}", resource.Name);
            }
            else
            {
                throw new Exception($"Resource {resource.Name} does not support reconfiguration");
                //_logger.LogDebug("Resource {ResourceName} does not support reconfiguration", resource.Name);
            }
        }
    }
}