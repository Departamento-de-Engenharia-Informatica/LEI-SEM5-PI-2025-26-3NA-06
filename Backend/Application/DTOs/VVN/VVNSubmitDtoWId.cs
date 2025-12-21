namespace ProjArqsi.Application.DTOs.VVN
{
    public class VVNSubmitDtoWId
    {
        public required string Id { get; set; }
        public required string ReferredVesselId { get; set; } 
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        
        // Optional cargo manifests
        public CargoManifestDto? LoadingManifest { get; set; }
        public CargoManifestDto? UnloadingManifest { get; set; }
        
        //public List<CrewMemberFinishedDto> CrewMembers { get; set; }
    }

    
}
