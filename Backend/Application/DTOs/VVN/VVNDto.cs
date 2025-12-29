namespace ProjArqsi.Application.DTOs.VVN
{
    //Usado para passar VVNs completas
    public class VVNDto
    {
        public required string Id { get; set; }
        public required string ReferredVesselId { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        
        // Cargo manifests
        public CargoManifestDto? LoadingManifest { get; set; }
        public CargoManifestDto? UnloadingManifest { get; set; }
        
        //public List<CrewMemberDto> CrewMembers { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        public string? TempAssignedDockId { get; set; }
        public bool IsHazardous { get; set; }
        public int EstimatedTeu { get; set; }
    }
}
