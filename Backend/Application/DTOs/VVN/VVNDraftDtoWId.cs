namespace ProjArqsi.Application.DTOs.VVN
{
    public class VVNDraftDtoWId
    {
        public required string Id { get; set; }
        public required string ReferredVesselId { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        //public List<CrewMemberDraftDto>? CrewMembers { get; set; }
        //public List<string>? CargoManifest { get; set; }
    }

    
}
