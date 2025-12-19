namespace ProjArqsi.Application.DTOs
{
    public class ContainerDto
    {
        public required string Id { get; set; }
        public required string IsoCode { get; set; }
        public required bool IsHazardous { get; set; }
        public required string CargoType { get; set; }
        public required string Description { get; set; }
    }
}
