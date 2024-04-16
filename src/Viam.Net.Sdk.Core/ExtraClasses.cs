using System;
using System.Collections.Generic;
using System.Text;

namespace Viam.Robot.V1
{
    public record CloudMetadata(
        string PrimaryOrgId,
        string LocationId,
        string MachineId,
        string MachinePartId,
        string RobotPartId);
}
