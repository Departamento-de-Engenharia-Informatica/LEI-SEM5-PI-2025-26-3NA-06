namespace ProjArqsi.SchedulingApi.DTOs
{
    // Response DTOs for Scheduling API

    public class ScheduleRequestDto
    {
        public DateTime TargetDate { get; set; }
    }

    public class ScheduleResponseDto
    {
        public DateTime TargetDate { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public List<OperationPlanDto> OperationPlans { get; set; } = new();
    }

    public class OperationPlanDto
    {
        public Guid VesselVisitNotificationId { get; set; }
        public string VesselName { get; set; } = string.Empty;
        public string VesselImo { get; set; } = string.Empty;
        public string AssignedDock { get; set; } = string.Empty;
        public DateTime PlannedStartTime { get; set; }
        public DateTime PlannedEndTime { get; set; }
        public List<CargoOperationDto> CargoOperations { get; set; } = new();
    }

    public class CargoOperationDto
    {
        public string OperationType { get; set; } = string.Empty; // "Loading" or "Unloading"
        public string ContainerId { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? AssignedCrane { get; set; }
    }
}
