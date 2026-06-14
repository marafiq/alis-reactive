namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The Resident Care Workspace for one resident. The workspace is a tabbed
    /// surface (Care Schedule, Medications, Incident Reports, Billing); the model
    /// names the resident the coordinator is working with.
    /// </summary>
    public class TabModel
    {
        public string ResidentName { get; set; } = "Jane Doe";

        public string RoomNumber { get; set; } = "204";
    }
}
