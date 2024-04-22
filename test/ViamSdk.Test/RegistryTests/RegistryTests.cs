using Viam.Net.Sdk.Core.Resources;
using Viam.Net.Sdk.Core.Resources.Components;

namespace ViamSdk.Test.RegistryTests
{
    public class RegistryTests
    {
        [Test]
        public void TestStaticRegistrations()
        {
            var arm = Registry.LookupSubtype(Arm.SubType);
            Assert.IsNotNull(arm);

            var sensor = Registry.LookupSubtype(Sensor.SubType);
            Assert.IsNotNull(sensor);
        }
    }
}
