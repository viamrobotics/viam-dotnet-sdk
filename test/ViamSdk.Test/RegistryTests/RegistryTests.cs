using Microsoft.Extensions.Logging;

using Viam.Core.Resources;
using Viam.Core.Resources.Components;

namespace Viam.Core.Test.RegistryTests
{
    public class RegistryTests
    {
        [Test]
        public void TestStaticRegistrations()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            Logging.Logger.SetLoggerFactory(loggerFactory);
            var arm = Registry.GetResourceRegistrationBySubtype(Arm.SubType);
            Assert.IsNotNull(arm);

            var sensor = Registry.GetResourceRegistrationBySubtype(Sensor.SubType);
            Assert.IsNotNull(sensor);
        }
    }
}
