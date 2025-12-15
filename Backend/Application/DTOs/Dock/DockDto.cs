namespace ProjArqsi.Application.DTOs.Dock
{
    public class DockDto
    {
        public required string Id { get; set; }
        public required string DockName { get; set; }
        public required string LocationDescription { get; set; }
        public required double Length { get; set; }
        public required double Depth { get; set; }
        public required double MaxDraft { get; set; }
        public required List<string> AllowedVesselTypeIds { get; set; }
    }
}
