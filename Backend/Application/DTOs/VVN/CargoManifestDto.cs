namespace ProjArqsi.Application.DTOs.VVN
{
    // DTO for a single manifest entry
    public class ManifestEntryDto
    {
        public required string ContainerId { get; set; }
        public string? ContainerIsoCode { get; set; }
        public string? SourceStorageAreaId { get; set; }
        public string? SourceStorageAreaName { get; set; }
        public string? TargetStorageAreaId { get; set; }
        public string? TargetStorageAreaName { get; set; }
    }

    // DTO for a cargo manifest (loading or unloading)
    public class CargoManifestDto
    {
        public required string ManifestType { get; set; } // "Load" or "Unload"
        public List<ManifestEntryDto> Entries { get; set; } = [];
    }
}
