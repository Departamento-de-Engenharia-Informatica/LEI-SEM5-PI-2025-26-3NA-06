namespace ProjArqsi.Application.DTOs.VVN
{
    public class DockConflictInfoDto
    {
        public bool HasConflicts { get; set; }
        public List<ConflictingVvnDto> ConflictingVvns { get; set; } = new();
    }

    public class ConflictingVvnDto
    {
        public string VvnId { get; set; } = string.Empty;
        public string VesselImo { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
    }
}
