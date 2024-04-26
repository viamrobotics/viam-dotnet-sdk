using Viam.Component.Gripper.V1;

namespace Viam.Core.Resources.Services
{
    internal class Gripper : GripperService.GripperServiceBase, IServiceBase
    {
        public string ServiceName => "viam.component.gripper.v1.GripperService";
    }
}
