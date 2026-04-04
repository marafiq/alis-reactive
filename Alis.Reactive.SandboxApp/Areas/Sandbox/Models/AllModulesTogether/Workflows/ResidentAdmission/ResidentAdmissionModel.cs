namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class ResidentAdmissionModel
    {
        public string? ResidentName { get; set; }
        public string? Physician { get; set; }
        public string? CareLevel { get; set; }
        public decimal? MonthlyRate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
    }
}
