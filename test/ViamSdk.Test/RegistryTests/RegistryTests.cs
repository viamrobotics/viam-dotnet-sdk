using Microsoft.Extensions.Logging;

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

            //var arm = Registry.GetResourceRegistrationBySubtype(ArmClient.SubType);
            //Assert.IsNotNull(arm);

            //var sensor = Registry.GetResourceRegistrationBySubtype(SensorClient.SubType);
            //Assert.IsNotNull(sensor);
        }
    }
}