using Viam.Component.Servo.V1;

namespace Viam.Core.Resources.Services
{
    internal class Servo : ServoService.ServoServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.servo.v1.ServoService";
    }
}
