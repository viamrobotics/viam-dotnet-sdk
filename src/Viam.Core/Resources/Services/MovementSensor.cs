using Viam.Component.Movementsensor.V1;

namespace Viam.Core.Resources.Services
{
    internal class MovementSensor : MovementSensorService.MovementSensorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.movementsensor.v1.MovementSensorService";
    }
}
