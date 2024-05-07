namespace Viam.Core
{
    public record CloudMetadata(
        string PrimaryOrgId,
        string LocationId,
        string MachineId,
        string MachinePartId,
        string RobotPartId)
    {
        public override string ToString() => $"{nameof(PrimaryOrgId)}:{PrimaryOrgId}, {nameof(LocationId)}:{LocationId}, {nameof(MachineId)}:{MachineId}, {nameof(MachinePartId)}:{MachinePartId}, {nameof(RobotPartId)}:{RobotPartId}";
    }
}
