namespace ProjArqsi.Application.DTOs.StorageArea
{
    public class StorageAreaUpsertDto
    {
        public required string AreaName { get; set; }
        public required string AreaType { get; set; }
        public required string Location { get; set; }
        public required int MaxCapacity { get; set; }
        public required bool ServesEntirePort { get; set; }
        public required List<Guid> ServedDockIds { get; set; }
    }
}
