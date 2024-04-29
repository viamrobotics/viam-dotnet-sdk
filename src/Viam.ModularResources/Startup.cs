using Grpc.Core;
using Grpc.Core.Interceptors;

using Viam.Core.Resources;
using Viam.ModularResources.Services;

using Arm = Viam.Core.Resources.Services.Arm;
using Base = Viam.Core.Resources.Services.Base;
using Board = Viam.Core.Resources.Services.Board;
using Camera = Viam.Core.Resources.Services.Camera;
using Encoder = Viam.Core.Resources.Services.Encoder;
using Gantry = Viam.Core.Resources.Services.Gantry;
using Generic = Viam.Core.Resources.Services.Generic;
using Gripper = Viam.Core.Resources.Services.Gripper;
using InputController = Viam.Core.Resources.Services.InputController;
using Motor = Viam.Core.Resources.Services.Motor;
using MovementSensor = Viam.Core.Resources.Services.MovementSensor;
using PowerSensor = Viam.Core.Resources.Services.PowerSensor;
using Sensor = Viam.Core.Resources.Services.Sensor;
using Servo = Viam.Core.Resources.Services.Servo;

namespace Viam.ModularResources
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc(o => o.Interceptors.Add<PopulateResourceByNameInterceptor>());
            services.AddGrpcReflection();
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
            e.MapGrpcService<Arm>();
            e.MapGrpcService<Base>();
            e.MapGrpcService<Board>();
            e.MapGrpcService<Camera>();
            e.MapGrpcService<Encoder>();
            e.MapGrpcService<Gantry>();
            e.MapGrpcService<Generic>();
            e.MapGrpcService<Gripper>();
            e.MapGrpcService<InputController>();
            e.MapGrpcService<Motor>();
            e.MapGrpcService<MovementSensor>();
            e.MapGrpcService<PowerSensor>();
            e.MapGrpcService<Sensor>();
            e.MapGrpcService<Servo>();
        }
    }

    internal class PopulateResourceByNameInterceptor(ILogger<PopulateResourceByNameInterceptor> logger, ResourceManager resourceManager) : Interceptor
    {
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            var name = GetResourceName(request);
            if (name != null)
            {
                logger.LogTrace("Found resource name field.");
                var resource = resourceManager.GetResourceByShortName(name);
                logger.LogTrace("Added {ResourceName} to UserState", resource.ResourceName);
                context.UserState.Add(nameof(resource), resource);
            }

            var boardName = GetBoardName(request);
            if (boardName != null)
            {
                logger.LogTrace("Found board name field.");
                var resource = resourceManager.GetResourceByShortName(boardName);
                logger.LogTrace("Added {ResourceName} to UserState", resource.ResourceName);
                context.UserState.Add("board", resource);
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
