namespace ProjArqsi.Application.DTOs.VVN
{
    //Usado para passar VVNs completas
    public class VVNDto
    {
        public string Id { get; set; } = string.Empty;
        public string ReferredVesselId { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        //public List<CrewMemberDto> CrewMembers { get; set; } = new();
        //public List<string> CargoManifest { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
    }

    public class CrewMemberDto
    {
        public string Name { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
