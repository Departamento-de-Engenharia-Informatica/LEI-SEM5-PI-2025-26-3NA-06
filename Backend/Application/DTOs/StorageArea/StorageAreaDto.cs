namespace ProjArqsi.Application.DTOs.StorageArea
{
    public class StorageAreaDto
    {
        public required string Id { get; set; }
        public required string AreaName { get; set; }
        public required string AreaType { get; set; }
        public required string Location { get; set; }
        public required int MaxCapacity { get; set; }
        public required bool ServesEntirePort { get; set; }
        public required List<string> ServedDockIds { get; set; }
    }
}
