namespace Viam.ModularResources
{
    public class Module(IHost host)
    {
        private readonly ILogger<Module> _logger =
            host.Services.GetRequiredService<ILoggerFactory>().CreateLogger<Module>();

        public async ValueTask Start(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Starting Module");
            await host.StartAsync(cancellationToken);
        }

        public async ValueTask Run(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Running Module");
            await host.RunAsync(cancellationToken);
        }

        public async ValueTask Stop(CancellationToken cancellationToken = default)
        {
            _logger.LogTrace("Stopping Module");
            await host.StopAsync(cancellationToken);
        }
    }
}