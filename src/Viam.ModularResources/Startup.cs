using Grpc.Core;
using Grpc.Core.Interceptors;
using Viam.Core.Resources.Components.Generic;
using Viam.Core.Resources.Components.Gripper;
using Viam.Core.Resources.Components.InputController;
using Viam.Core.Resources.Components.Motor;
using Viam.Core.Resources.Components.MovementSensor;
using Viam.Core.Resources.Components.PowerSensor;
using Viam.Core.Resources.Components.Sensor;
using Viam.Core.Resources.Components.Servo;
using Viam.ModularResources.Services;
using ArmService = Viam.Core.Resources.Components.Arm.ArmService;
using BaseService = Viam.Core.Resources.Components.Base.BaseService;
using BoardService = Viam.Core.Resources.Components.Board.BoardService;
using CameraService = Viam.Core.Resources.Components.Camera.CameraService;
using EncoderService = Viam.Core.Resources.Components.Encoder.EncoderService;
using GantryService = Viam.Core.Resources.Components.Gantry.GantryService;

namespace Viam.ModularResources
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(o => o.Interceptors.Add<PopulateResourceByNameInterceptor>());
            services.AddGrpcReflection();
            services.AddSingleton<ResourceManager>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(e =>
            {
                e.MapGrpcService<SignalingService>();
                e.MapGrpcService<RobotService>();
                e.MapGrpcService<ModuleService>();
                MapComponentGrpcServices(e);
                e.MapGrpcReflectionService();
            });
        }

        private void MapComponentGrpcServices(IEndpointRouteBuilder e)
        {
            if (e.ServiceProvider.GetService<ArmService>() != null)
                e.MapGrpcService<ArmService>();

            if (e.ServiceProvider.GetService<BaseService>() != null)
                e.MapGrpcService<BaseService>();

            if (e.ServiceProvider.GetService<BoardService>() != null)
                e.MapGrpcService<BoardService>();

            if (e.ServiceProvider.GetService<CameraService>() != null)
                e.MapGrpcService<CameraService>();

            if (e.ServiceProvider.GetService<EncoderService>() != null)
                e.MapGrpcService<EncoderService>();

            if (e.ServiceProvider.GetService<GantryService>() != null)
                e.MapGrpcService<GantryService>();

            if (e.ServiceProvider.GetService<GenericService>() != null)
                e.MapGrpcService<GenericService>();

            if (e.ServiceProvider.GetService<GripperService>() != null)
                e.MapGrpcService<GripperService>();

            if (e.ServiceProvider.GetService<InputControllerService>() != null)
                e.MapGrpcService<InputControllerService>();

            if (e.ServiceProvider.GetService<MotorService>() != null)
                e.MapGrpcService<MotorService>();

            if (e.ServiceProvider.GetService<MovementSensorService>() != null)
                e.MapGrpcService<MovementSensorService>();

            if (e.ServiceProvider.GetService<PowerSensorService>() != null)
                e.MapGrpcService<PowerSensorService>();

            if (e.ServiceProvider.GetService<SensorService>() != null)
                e.MapGrpcService<SensorService>();

            if (e.ServiceProvider.GetService<ServoService>() != null)
                e.MapGrpcService<ServoService>();
        }
    }

    internal class PopulateResourceByNameInterceptor(
        ILogger<PopulateResourceByNameInterceptor> logger,
        ResourceManager resourceManager) : Interceptor
    {
        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            var name = GetResourceName(request);
            if (name != null)
            {
                logger.LogTrace("Found resource name field.");
                logger.LogDebug("Adding {ResourceName} to UserState", name);
                var resource = resourceManager.GetService(name);
                logger.LogTrace("Added {ResourceName} to UserState", resource.Name);
                context.UserState.Add(nameof(resource), resource);
            }

            var boardName = GetBoardName(request);
            if (boardName != null)
            {
                logger.LogTrace("Found board name field.");
                //var resource = resourceManager.GetResourceByShortName(boardName);
                //logger.LogTrace("Added {ResourceName} to UserState", resource.ResourceName);
                //context.UserState.Add("board", resource);
            }

            await continuation(request, responseStream, context);
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var name = GetResourceName(request);
            if (name != null)
            {
                logger.LogTrace("Found resource name field.");
                logger.LogDebug("Adding {ResourceName} to UserState", name);
                var resource = resourceManager.GetService(name);
                logger.LogTrace("Added {ResourceName} to UserState", resource.Name);
                context.UserState.Add(nameof(resource), resource);
            }

            var boardName = GetBoardName(request);
            if (boardName != null)
            {
                logger.LogTrace("Found board name field.");
                //var resource = resourceManager.GetResourceByShortName(boardName);
                //logger.LogTrace("Added {ResourceName} to UserState", resource.ResourceName);
                //context.UserState.Add("board", resource);
            }

            return await continuation(request, context);
        }

        private static string? GetResourceName<TRequest>(TRequest request)
        {
            var property = typeof(TRequest).GetProperty("Name");
            if (property == null)
            {
                return null;
            }

            if (property.GetValue(request) is not string name)
            {
                return null;
            }

            return property.GetValue(request) as string;
        }

        private static string? GetBoardName<TRequest>(TRequest request)
        {
            var property = typeof(TRequest).GetProperty("BoardName");
            if (property == null)
            {
                return null;
            }

            if (property.GetValue(request) is not string name)
            {
                return null;
            }

            return property.GetValue(request) as string;
        }
    }
}