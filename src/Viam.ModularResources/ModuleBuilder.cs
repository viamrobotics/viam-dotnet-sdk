using Microsoft.AspNetCore.Server.Kestrel.Core;

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
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
using Viam.ModularResources.Logging;

namespace Viam.ModularResources
{
    public class ModuleBuilder
    {
        private readonly IHostBuilder _hostBuilder;
        private readonly IList<Action<IServiceProvider>> _postConfigureActions = [];

        public static ModuleBuilder FromArgs(string[] args)
        {
            return new ModuleBuilder(args);
        }

        public ModuleBuilder(string[] args)
        {
            if (args.Length != 1)
                throw new ArgumentException("You must provide a Unix socket path");
            _hostBuilder = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) => config
                    .AddEnvironmentVariables()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true))
                .ConfigureServices((c, s) =>
                {
                    
                })
                .ConfigureLogging(c =>
                {
                    c.ClearProviders();
                    c.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<ViamLoggerProviderOptions>>().Value);
                    c.Services.AddSingleton<ViamLoggerProvider>();
                    c.Services.AddSingleton<ILoggerProvider>(sp => sp.GetRequiredService<ViamLoggerProvider>());
                    c.Services.AddHostedService<ViamLoggerSinkWorker>();
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

        public ModuleBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureAppConfiguration(configureDelegate);
            return this;
        }

        public ModuleBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _hostBuilder.ConfigureServices(configureDelegate);
            return this;
        }

        public ModuleBuilder ConfigureServices(Action<IServiceCollection> configureDelegate)
        {
            _hostBuilder.ConfigureServices(configureDelegate);
            return this;
        }

        public ModuleBuilder RegisterService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TService>()
            where TService : class, IComponentServiceBase
        {
            _hostBuilder.ConfigureServices(s =>
            {
                s.AddSingleton<TService>();
                //s.AddSingleton<IComponentServiceBase, TService>(s => s.GetRequiredService<TService>());
            });
            return this;
        }

        public ModuleBuilder RegisterComponent<
            TComponentInterface,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
            TComponent>()
            where TComponentInterface : class, IComponentBase
            where TComponent : class, IResourceBase, TComponentInterface, IModularResourceService
        {
            _hostBuilder.ConfigureServices(s =>
            {
                //s.AddKeyedTransient<IModularResource, TComponent>(TComponent.SubType);
                _postConfigureActions.Add(RegisterTypeHelper<TComponentInterface, TComponent>);
            });
            return this;
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

        private static void RegisterTypeHelper<
                TComponentInterface,
                [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)]
                TComponent>
            (IServiceProvider services)
            where TComponentInterface : class, IComponentBase
            where TComponent : class, IResourceBase, TComponentInterface, IModularResourceService
        {
            var manager = services.GetRequiredService<ResourceManager>();
            manager.RegisterType<TComponentInterface, TComponent>();
        }
    }
}