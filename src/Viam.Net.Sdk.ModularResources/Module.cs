using System.Buffers;
using System.Net;
using System.Net.Sockets;
using Viam.Core.Resources;

namespace Viam.ModularResources
{
    public class Module(ILogger logger)
    {
        private readonly ILogger _logger = logger;
        private readonly SemaphoreSlim _semaphore = new(1);
        private bool _ready;

        public static Module FromArgs()
        {
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            var logger = loggerFactory.CreateLogger<Module>();
            //return new Module(new Server(logger), logger);
            return null;
        }

        public async ValueTask Start()
        {
        }

        public async ValueTask Stop()
        {

        }

        public void SetReady(bool ready)
        {
            _ready = ready;
        }

        public async ValueTask AddResource()
        {

        }

        public async ValueTask ReconfigureResource()
        {

        }

        public async ValueTask RemoveResource()
        {

        }

        public async ValueTask Ready()
        {

        }

        public void AddModelFromRegistry(SubType subType, Model model)
        {

        }

        public async ValueTask ValidateConfig()
        {

        }
    }
}
