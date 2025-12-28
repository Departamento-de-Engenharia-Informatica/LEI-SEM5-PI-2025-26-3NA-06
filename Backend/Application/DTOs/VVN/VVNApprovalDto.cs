namespace ProjArqsi.Application.DTOs.VVN
{
    public class VVNApprovalDto
    {
        public required string TempAssignedDockId { get; set; }
        public bool ConfirmDockConflict { get; set; } = false;
    }
}
