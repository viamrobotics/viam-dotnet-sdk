using Viam.Component.Powersensor.V1;

namespace Viam.Core.Resources.Services
{
    internal class PowerSensor : PowerSensorService.PowerSensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.powersensor.v1.PowerSensorService";
    }
}
