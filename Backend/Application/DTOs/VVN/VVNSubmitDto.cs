namespace ProjArqsi.Application.DTOs.VVN
{
    public class VVNSubmitDto
    {
        public required string ReferredVesselId { get; set; } 
        public required DateTime ArrivalDate { get; set; }
        public required DateTime DepartureDate { get; set; }
        //public List<CrewMemberFinishedDto> CrewMembers { get; set; }
        //public List<string> CargoManifest { get; set; }
        
        
    }

    
}
