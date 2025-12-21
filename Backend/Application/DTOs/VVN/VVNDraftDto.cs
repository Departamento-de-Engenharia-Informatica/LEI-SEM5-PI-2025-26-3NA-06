namespace ProjArqsi.Application.DTOs.VVN
{
    public class VVNDraftDto
    {
        public required string ReferredVesselId { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        
        // Optional cargo manifests for draft
        public CargoManifestDto? LoadingManifest { get; set; }
        public CargoManifestDto? UnloadingManifest { get; set; }
        
        //public List<CrewMemberDraftDto>? CrewMembers { get; set; }
    }

    
}
