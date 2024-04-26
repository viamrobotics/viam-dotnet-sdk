namespace Viam.Core
{
    public record CloudMetadata(
        string PrimaryOrgId,
        string LocationId,
        string MachineId,
        string MachinePartId,
        string RobotPartId);
}
