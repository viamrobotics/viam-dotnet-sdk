using Grpc.Core;

using Viam.Core;
using Viam.Core.Clients;
using Viam.Core.Grpc;
using Viam.Core.Logging;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;
using Viam.Core.Utils;
using Viam.Module.V1;
using Viam.Robot.V1;

using grpcRobotService = Viam.Module.V1.ModuleService;

namespace Viam.ModularResources.Services
{
    public class ModuleService(IServiceProvider services, ResourceManager manager, ILoggerFactory loggerFactory) : grpcRobotService.ModuleServiceBase
    {
        private static readonly SemaphoreSlim ParentAddressLock = new(1);
        private static Uri? _parentAddress;
        private readonly ILogger<ModuleService> _logger = loggerFactory.CreateLogger<ModuleService>();
        public readonly ResourceManager Manager = manager;

        
        public override async Task<AddResourceResponse> AddResource(AddResourceRequest request, ServerCallContext context)
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
                var client = new RobotClientBase(loggerFactory, channel);
                var remoteResourceNames = await client.ResourceNamesAsync();
                Manager.RegisterRemoteResources(remoteResourceNames, channel);

                var creator = Registry.GetResourceCreatorRegistration(subType, model);

                var dependencies = request.Dependencies.Select(GrpcExtensions.ToResourceName)
                                          .ToDictionary(x => x, x => Manager.GetResource(x));

                var resource = creator.Creator(loggerFactory.CreateLogger(config.Name), config, dependencies);
                _logger.LogDebug("Registering {ResourceName} with the ResourceManager", resource.ResourceName);
                Manager.Register(resource.ResourceName, resource);
                return new AddResourceResponse();
            }
            catch (ResourceCreatorRegistrationNotFoundException ex)
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
            
            var serviceNameToModels = new Dictionary<(string, SubType), ICollection<Model>>();

            foreach (var (subType, model) in Registry.RegisteredResourceCreators)
            {
                _logger.LogModularServiceLookingForService(subType, model);

                var s = services.GetServices<IServiceBase>().ToArray();
                var service = s.FirstOrDefault(x => x.SubType == subType);
                if (service == null)
                {
                    _logger.LogModularServiceMissingService(subType, s.ToArray());
                    continue;
                }
                var models = serviceNameToModels.GetValueOrDefault((service.ServiceName, subType), new List<Model>());
                models.Add(model);
                serviceNameToModels[(service.ServiceName, subType)] = models;
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

            var resp = new ReadyResponse
            {
                Ready = true,
                Handlermap = handlers
            };

            _logger.LogDebug("Done preparing ready response");
            return resp;
        }

        
        public override async Task<ReconfigureResourceResponse> ReconfigureResource(ReconfigureResourceRequest request, ServerCallContext context)
        {
            // TODO: Need to refresh the manager resources before doing this?
            var dependencies = request.Dependencies.Select(GrpcExtensions.ToResourceName)
                                      .ToDictionary(x => x, x => Manager.GetResource(x));
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
                _logger.LogDebug("Available Resources: {AvailableResources}", Manager.GetResourceNames());
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
                var creator = Registry.GetResourceCreatorRegistration(subtype, model);
                _logger.LogDebug("Creator found, invoking creator for {ResourceName}", resourceName);
                resource = creator.Creator(loggerFactory.CreateLogger(request.Config.Name), request.Config, dependencies);
                Manager.Register(resourceName, resource);
            }

            _logger.LogDebug("Done reconfiguring {ResourceName}", resourceName);

            return new ReconfigureResourceResponse();
        }

        
        public override Task<RemoveResourceResponse> RemoveResource(RemoveResourceRequest request, ServerCallContext context)
        {
            Manager.RemoveResource(request.Name.ToResourceName());
            return Task.FromResult(new RemoveResourceResponse());
        }

        
        public override Task<ValidateConfigResponse> ValidateConfig(ValidateConfigRequest request, ServerCallContext context)
        {
            var config = request.Config;
            var subType = SubType.FromString(config.Api);
            var model = Model.FromString(config.Model);
            var validator = Registry.GetResourceCreatorRegistration(subType, model).ConfigValidator;
            var dependencies = validator(config);
            var resp = new ValidateConfigResponse();
            resp.Dependencies.AddRange(dependencies);
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
    }
}
