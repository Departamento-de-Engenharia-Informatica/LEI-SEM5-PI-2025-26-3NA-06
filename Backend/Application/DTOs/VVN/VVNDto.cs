namespace ProjArqsi.Application.DTOs.VVN
{
    //Usado para passar VVNs completas
    public class VVNDto
    {
        public required string Id { get; set; }
        public required string ReferredVesselId { get; set; }
        public required DateTime ArrivalDate { get; set; }
        public required DateTime DepartureDate { get; set; }
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
