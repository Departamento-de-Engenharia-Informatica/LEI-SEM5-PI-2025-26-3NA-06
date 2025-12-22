namespace ProjArqsi.SchedulingApi.DTOs
{
    // DTOs for communication with Core API
    
    public class VesselVisitNotificationDto
    {
        public Guid Id { get; set; }
        public string ReferredVesselId { get; set; } = string.Empty;
        public DateTime? ArrivalDate { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string? TempAssignedDockId { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<CargoManifestDto>? CargoManifests { get; set; }
        public int EstimatedTeu { get; set; }
    }

    public class CargoManifestDto
    {
        public string ManifestType { get; set; } = string.Empty;
        public List<CargoItemDto> CargoItems { get; set; } = new();
    }

    public class CargoItemDto
    {
        public string ContainerId { get; set; } = string.Empty;
        public string? OriginStorageAreaId { get; set; }
        public string? DestinationStorageAreaId { get; set; }
        public int Quantity { get; set; }
    }

    public class DockDto
    {
        public Guid Id { get; set; }
        public string DockName { get; set; } = string.Empty;
        public double Length { get; set; }
        public double Depth { get; set; }
        public double MaxDraft { get; set; }
    }

    public class StorageAreaDto
    {
        public string Id { get; set; } = string.Empty;
        public string AreaName { get; set; } = string.Empty;
        public string AreaType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int MaxCapacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public bool ServesEntirePort { get; set; }
        public List<string> ServedDockIds { get; set; } = new();
    }

    public class VesselDto
    {
        public Guid Id { get; set; }
        public string Imo { get; set; } = string.Empty;
        public string VesselName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public double Length { get; set; }
    }
}
