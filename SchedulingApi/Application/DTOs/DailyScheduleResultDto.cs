namespace ProjArqsi.SchedulingApi.DTOs
{
    public class DailyScheduleResultDto
    {
        public string Date { get; set; } = string.Empty;
        public bool IsFeasible { get; set; }
        public List<string> Warnings { get; set; } = [];
        public List<DockAssignmentDto> Assignments { get; set; } = [];
    }

    public class DockAssignmentDto
    {
        public Guid VvnId { get; set; }
        public Guid VesselId { get; set; }
        public string? VesselImo { get; set; }
        public string? VesselName { get; set; }
        public Guid DockId { get; set; }
        public string? DockName { get; set; }
        public DateTime Eta { get; set; }
        public DateTime Etd { get; set; }
        public int EstimatedTeu { get; set; }
    }
}
