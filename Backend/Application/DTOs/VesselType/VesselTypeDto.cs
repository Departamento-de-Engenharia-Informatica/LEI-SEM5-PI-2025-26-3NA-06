namespace ProjArqsi.Application.DTOs
{
    public class VesselTypeDto
    {
        public required string Id { get; set; }
        public required string TypeName { get; set; }
        public required string TypeDescription { get; set; }
        public required int TypeCapacity { get; set; }
        public required int MaxRows { get; set; }
        public required int MaxBays { get; set; }
        public required int MaxTiers { get; set; }
    }
}
