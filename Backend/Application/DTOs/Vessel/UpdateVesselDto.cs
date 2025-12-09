namespace ProjArqsi.Application.DTOs
{
    public class UpdateVesselDto
    {
        public required string VesselName { get; set; }
        public required int Capacity { get; set; }
        public required int Rows { get; set; }
        public required int Bays { get; set; }
        public required int Tiers { get; set; }
        public required double Length { get; set; }
        public required Guid VesselTypeId { get; set; }
    }
}