namespace ProjArqsi.Application.DTOs
{
    public class CreateVesselTypeDto
    {
        public string TypeName { get; set; } = string.Empty;
        public string TypeDescription { get; set; } = string.Empty;
        public int TypeCapacity { get; set; }
        public int MaxRows { get; set; }
        public int MaxBays { get; set; }
        public int MaxTiers { get; set; }
    }
}
