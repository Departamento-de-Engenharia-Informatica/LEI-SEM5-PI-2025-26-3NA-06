namespace ProjArqsi.Application.DTOs
{
    public class VesselDto
    {
        public required string Id { get; set; }
        public required string Imo { get; set; }
        public required string VesselName { get; set; }
        public required int Capacity { get; set; }
        public required int Rows { get; set; }
        public required int Bays { get; set; }
        public required int Tiers { get; set; }
        public required double Length { get; set; }
        public required string VesselTypeId { get; set; }
        public required string VesselTypeName { get; set; }
    }
}