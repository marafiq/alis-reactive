namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Assignment edit form partial loaded into a NativeDrawer from schedule clicks.
    /// </summary>
    public class EditAssignmentModel
    {
        public int AssignmentId { get; set; }
        public int? StaffId { get; set; }
        public string? Notes { get; set; }
    }
}
