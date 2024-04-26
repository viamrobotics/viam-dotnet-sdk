using Viam.Component.Motor.V1;

namespace Viam.Core.Resources.Services
{
    internal class Motor : MotorService.MotorServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.motor.v1.MotorService";
    }
}
