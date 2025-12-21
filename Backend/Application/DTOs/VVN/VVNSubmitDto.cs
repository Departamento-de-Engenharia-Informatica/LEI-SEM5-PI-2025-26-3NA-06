namespace ProjArqsi.Application.DTOs.VVN
{
    public class VVNSubmitDto
    {
        public required string ReferredVesselId { get; set; } 
        public required DateTime ArrivalDate { get; set; }
        public required DateTime DepartureDate { get; set; }
        
        // Optional cargo manifests for submission (both can be null)
        public CargoManifestDto? LoadingManifest { get; set; }
        public CargoManifestDto? UnloadingManifest { get; set; }
        
        //public List<CrewMemberFinishedDto> CrewMembers { get; set; }
    }

    
}
