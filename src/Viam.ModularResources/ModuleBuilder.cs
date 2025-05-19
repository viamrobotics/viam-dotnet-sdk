using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Diagnostics.CodeAnalysis;
using Viam.Core.Resources;
using Viam.Core.Resources.Components;
using Viam.Core.Resources.Components.Arm;
using Viam.Core.Resources.Components.Base;
using Viam.Core.Resources.Components.Board;
using Viam.Core.Resources.Components.Camera;
using Viam.Core.Resources.Components.Encoder;
using Viam.Core.Resources.Components.Gantry;
using Viam.Core.Resources.Components.Generic;
using Viam.Core.Resources.Components.Gripper;
using Viam.Core.Resources.Components.InputController;
using Viam.Core.Resources.Components.Motor;
using Viam.Core.Resources.Components.MovementSensor;
using Viam.Core.Resources.Components.PowerSensor;
using Viam.Core.Resources.Components.Sensor;
using Viam.Core.Resources.Components.Servo;

namespace Viam.ModularResources
{
    public class ModuleBuilder
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly IList<Action<IServiceProvider>> _postConfigureActions = [];

        public static ModuleBuilder FromArgs(string[] args, ILoggerFactory? loggerFactory = null)
        {
            return new ModuleBuilder(args, loggerFactory);
        }

        public ModuleBuilder(string[] args, ILoggerFactory? loggerFactory = null)
        {
            if (args.Length != 1)
                throw new ArgumentException("You must provide a Unix socket path");
            _hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureServices((c, s) =>
                {
                    if (loggerFactory != null)
                    {
                        s.AddSingleton<ILoggerFactory>(loggerFactory);
                        s.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
                    }
                })
                .ConfigureLogging(c =>
                {
                    if (loggerFactory == null)
                    {
                        c.AddSimpleConsole(o =>
                        {
                            o.SingleLine = true;
                            o.IncludeScopes = true;
                            o.UseUtcTimestamp = true;
                        });
                    }
                    else
                    {
                        c.ClearProviders();
                    }
                })
                .ConfigureWebHostDefaults(b => { b.UseStartup<Startup>(); })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        var socket = args[0];
                        options.ListenUnixSocket(socket, c => c.Protocols = HttpProtocols.Http2);
                    });
                });
            RegisterService<ArmService>();
            RegisterService<BaseService>();
            RegisterService<BoardService>();
            RegisterService<CameraService>();
            RegisterService<EncoderService>();
            RegisterService<GantryService>();
            RegisterService<GenericService>();
            RegisterService<GripperService>();
            RegisterService<InputControllerService>();
            RegisterService<MotorService>();
            RegisterService<MovementSensorService>();
            RegisterService<PowerSensorService>();
            RegisterService<SensorService>();
            RegisterService<ServoService>();
        }

        public void ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate) =>
            _hostBuilder.ConfigureServices(configureDelegate);

        public void ConfigureServices(Action<IServiceCollection> configureDelegate) =>
            _hostBuilder.ConfigureServices(configureDelegate);

        public void RegisterService<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>()
            where TService : class, IServiceBase
        {
            _hostBuilder.ConfigureServices(s =>
            {
                s.AddSingleton<TService>();
                //s.AddSingleton<IServiceBase, TService>(s => s.GetRequiredService<TService>());
            });
        }

        public void RegisterComponent<TComponentInterface,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                        DynamicallyAccessedMemberTypes.PublicProperties)]
            TComponent>()
            where TComponentInterface : class, IComponentBase
            where TComponent : class, IResourceBase, TComponentInterface, IModularResourceService
        {
            _hostBuilder.ConfigureServices(s =>
            {
                s.AddKeyedTransient<IModularResource, TComponent>(TComponent.SubType);
                _postConfigureActions.Add(s => RegisterTypeHelper<TComponentInterface, TComponent>(s));
            });
        }

        public Module Build()
        {
            var host = _hostBuilder.Build();
            foreach (var action in _postConfigureActions)
            {
                action(host.Services);
            }

            return new Module(host);
        }

        private static void RegisterTypeHelper<TComponentInterface,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors |
                                        DynamicallyAccessedMemberTypes.PublicProperties)]
            TComponent>(IServiceProvider services)
            where TComponentInterface : class, IComponentBase
            where TComponent : class, IResourceBase, TComponentInterface, IModularResourceService
        {
            var manager = services.GetRequiredService<ResourceManager>();
            manager.RegisterType<TComponentInterface, TComponent>();
        }
    }
}