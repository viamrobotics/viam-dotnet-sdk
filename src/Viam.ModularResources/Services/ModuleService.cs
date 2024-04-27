using System.Diagnostics;
using Grpc.Core;

using Viam.Common.V1;
using Viam.Core.Grpc;
using Viam.Core.Resources;
using Viam.Core.Utils;
using Viam.Module.V1;
using Viam.Robot.V1;

using static Viam.Core.Utils.GrpcExtensions;

using grpcRobotService = Viam.Module.V1.ModuleService;

namespace Viam.ModularResources.Services
{
    public class ModuleService(ResourceManager manager, ILoggerFactory loggerFactory) : grpcRobotService.ModuleServiceBase
    {
        private readonly ILogger<ModuleService> _logger = loggerFactory.CreateLogger<ModuleService>();
        public readonly ResourceManager Manager = manager;

        public override Task<Viam.Module.V1.AddResourceResponse> AddResource(Viam.Module.V1.AddResourceRequest request, ServerCallContext context)
        {
            _logger.LogDebug("Starting AddResource...");
            var config = request.Config;
            var subType = SubType.FromString(config.Api);
            var model = Model.FromString(config.Model);
            _logger.LogDebug("Adding resource {Name} {SubType} {Model}", config.Name, subType, model);
            var creator = Registry.LookupResourceCreatorRegistration(subType, model);
            var resource = creator.Creator(config, [.. request.Dependencies]);
            _logger.LogDebug("Registering {ResourceName} with the ResourceManager", resource.ResourceName);
            Manager.Register(resource.ResourceName, resource);
            return Task.FromResult(new Viam.Module.V1.AddResourceResponse());
        }

        public override async Task<Viam.Module.V1.ReadyResponse> Ready(Viam.Module.V1.ReadyRequest request, ServerCallContext context)
        {
            _logger.LogDebug("Calling ready...");
            _logger.LogDebug("Dialing parent...");
            var dialer = new Viam.Core.Grpc.GrpcDialer(loggerFactory.CreateLogger<GrpcDialer>());
            // We need to force the correct URI format so we pre-pend file:///
            var options = new GrpcDialOptions(new Uri($"file:///{request.ParentAddress}"), true);
            var channel = await dialer.DialDirectAsync(options);
            _logger.LogDebug("Dialed parent, preparing resources...");
            var serviceNameToModels = new Dictionary<(string, SubType), ICollection<Model>>();
            foreach(var registeredCreator in Registry.RegisteredResourceCreators)
            {
                _logger.LogDebug("Loading resource creator for {SubType} {Model}",
                                 registeredCreator.SubType,
                                 registeredCreator.Model);
                var subType = registeredCreator.SubType;
                var model = registeredCreator.Model;
                var registration = Registry.LookupSubtype(subType);
                var service = registration.CreateServiceBase(Manager);
                var serviceName = service.ServiceName;

                var models = serviceNameToModels.GetValueOrDefault((serviceName, subType), new List<Model>());
                models.Add(model);
                serviceNameToModels[(serviceName, subType)] = models;
            }

            _logger.LogDebug("Preparing handlers...");
            var handlers = new HandlerMap();
            foreach (var ((serviceName, subType), models) in serviceNameToModels)
            {
                _logger.LogTrace("Preparing handler for {Service}", serviceName);
                var rpcSubtype = new ResourceRPCSubtype() { Subtype = new ViamResourceName(subType, string.Empty), ProtoService = serviceName };
                var handler = new HandlerDefinition() { Subtype = rpcSubtype };
                handler.Models.AddRange(models.Select(x => x.ToString()));
                handlers.Handlers.Add(handler);
            }

            var resp = new Viam.Module.V1.ReadyResponse
            {
                Ready = true,
                Handlermap = handlers
            };

            _logger.LogDebug("Done preparing ready response.");
            return resp;
        }

        public override async Task<Viam.Module.V1.ReconfigureResourceResponse> ReconfigureResource(Viam.Module.V1.ReconfigureResourceRequest request, ServerCallContext context)
        {
            // TODO: Need to refresh the manager resources before doing this?
            var dependencies = request.Dependencies.Select(GrpcExtensions.ToResourceName)
                                      .ToDictionary(x => x, Manager.GetResource);
            var subtype = SubType.FromString(request.Config.Api);
            _logger.LogDebug("SubType {SubType}", subtype);
            var resourceName = new ViamResourceName(subtype, request.Config.Name);
            _logger.LogInformation("Reconfigure request received for {ResourceName}", resourceName.ToString());
            IResourceBase resource;
            try
            {
                resource = Manager.GetResource(resourceName);
            }
            catch (ResourceNotFoundException)
            {
                _logger.LogDebug("Available Resources: {AvailableResources}", manager.GetResourceNames());
                throw;
            }

            // If the resource implements IAsyncReconfigurable, execute it, otherwise remove/add it
            if (resource is IAsyncReconfigurable reconfigurable)
            {
                _logger.LogDebug("Resource {ResourceName} supports reconfiguration", resourceName);
                await reconfigurable.Reconfigure(request.Config, dependencies);
                _logger.LogDebug("Reconfigured");
            }
            else
            {
                _logger.LogDebug("Resource {ResourceName} does not support reconfiguration, removing", resourceName);
                var model = Model.FromString(request.Config.Model);
                Manager.RemoveResource(resourceName);
                _logger.LogDebug("Disposing of resource {ResourceName}", resourceName);
                await resource.DisposeAsync();
                _logger.LogDebug("Looking up ResourceCreator in registry for {ResourceName}", resourceName);
                var creator = Registry.LookupResourceCreatorRegistration(subtype, model);
                _logger.LogDebug("Creator found, invoking creator for {ResourceName}", resourceName);
                resource = creator.Creator(request.Config, request.Dependencies.ToArray());
                Manager.Register(resourceName, resource);
            }

            _logger.LogDebug("Done reconfiguring {ResourceName}", resourceName);

            return new ReconfigureResourceResponse();
        }

        public override Task<Viam.Module.V1.RemoveResourceResponse> RemoveResource(Viam.Module.V1.RemoveResourceRequest request, ServerCallContext context)
        {
            Manager.RemoveResource(request.Name.ToResourceName());
            return Task.FromResult(new Viam.Module.V1.RemoveResourceResponse());
        }

        public override Task<Viam.Module.V1.ValidateConfigResponse> ValidateConfig(Viam.Module.V1.ValidateConfigRequest request, ServerCallContext context)
        {
            var config = request.Config;
            var subType = SubType.FromString(config.Api);
            var model = Model.FromString(config.Model);
            var validator = Registry.LookupResourceCreatorRegistration(subType, model).ConfigValidator;
            var dependencies = validator(config);
            var resp = new Viam.Module.V1.ValidateConfigResponse();
            resp.Dependencies.AddRange(dependencies);
            return Task.FromResult(resp);
        }
    }
}
