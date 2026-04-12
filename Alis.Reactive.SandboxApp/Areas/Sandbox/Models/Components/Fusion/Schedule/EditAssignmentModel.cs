namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Model for the assignment edit form partial.
    /// Loaded into a NativeDrawer when user clicks an assignment on the schedule.
    /// </summary>
    public class EditAssignmentModel
    {
        public int AssignmentId { get; set; }
        public int? StaffId { get; set; }
        public string? Notes { get; set; }
    }
}
