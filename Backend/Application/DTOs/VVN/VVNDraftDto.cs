namespace ProjArqsi.Application.DTOs.VVN
{
    public class VVNDraftDto
    {
        public required string ReferredVesselId { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        //public List<CrewMemberDraftDto>? CrewMembers { get; set; }
        //public List<string>? CargoManifest { get; set; }
    }

    
}
