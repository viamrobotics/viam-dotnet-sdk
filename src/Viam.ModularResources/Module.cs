using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Runtime.InteropServices;

using Viam.Core.Resources;

namespace Viam.ModularResources
{
    public class Module(IHost host)
    {
        private readonly ILogger<Module> _logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Module>();
        private readonly SemaphoreSlim _semaphore = new(1);
        private readonly IHost _host = host;

        public static Module FromArgs(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("We currently only support Modular Resources on Linux/macOS");
            if (args.Length != 1)
                throw new ArgumentException("You must provide a Unix socket path");
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(c =>
                {
                    c.AddSimpleConsole(o => {
                        o.SingleLine = true;
                        o.IncludeScopes = true;
                        o.UseUtcTimestamp = true;
                        });
                })
                .ConfigureServices(s =>
                {
                    s.AddSingleton<ResourceManager>();
                })
                .ConfigureWebHostDefaults(b =>
                {
                    b.UseStartup<Startup>();
                })
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        var socket = args[0];
                        options.ListenUnixSocket(socket, c => c.Protocols = HttpProtocols.Http2);
                        options.ListenLocalhost(5000, c => c.Protocols = HttpProtocols.Http2);
                    });
                })
                .Build();

            return new Module(host);
        }

        public async ValueTask Start(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Starting Module");
            await _host.StartAsync(cancellationToken);
        }

        public async ValueTask Run(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Running Module");
            await _host.RunAsync(cancellationToken);
        }

        public async ValueTask Stop(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Stopping Module");
            await _host.StopAsync(cancellationToken);
        }

        public void SetReady(bool ready)
        {
            //_ready = ready;
        }

        public ValueTask AddResource()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask ReconfigureResource()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveResource()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Ready()
        {
            return ValueTask.CompletedTask;
        }

        public void AddModelFromRegistry(SubType subType, Model model)
        {
            try
            {
                _logger.LogDebug("Adding model {Model} for {SubType}", model, subType);
                Registry.LookupResourceCreatorRegistration(subType, model);
            }
            catch (ResourceCreatorRegistrationNotFoundException)
            {
                _logger.LogError("Cannot add model because it has not been registered for {SubType} {Model}", subType, model);
                throw;
            }
        }

        public ValueTask ValidateConfig()
        {
            return ValueTask.CompletedTask;
        }
    }
}
