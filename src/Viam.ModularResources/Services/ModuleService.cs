using Grpc.Core;

using Viam.Common.V1;
using Viam.Core.Grpc;
using Viam.Core.Resources;
using Viam.Core.Utils;
using Viam.Module.V1;
using Viam.Robot.V1;

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
                var rpcSubtype = new ResourceRPCSubtype() { Subtype = new ResourceName() { Namespace = subType.Namespace, Type = subType.ResourceType, Subtype = subType.ResourceSubType, Name = "" }, ProtoService = serviceName };
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

        public override Task<Viam.Module.V1.ReconfigureResourceResponse> ReconfigureResource(Viam.Module.V1.ReconfigureResourceRequest request, ServerCallContext context)
        {
            return base.ReconfigureResource(request, context);
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
