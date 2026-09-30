namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A request to change a resident's care level. Whether a physician must sign off is decided
    /// by the server and travels with the form in a hidden field.
    /// </summary>
    public class CareLevelChangeModel
    {
        public int ResidentId { get; set; }
        public bool RequiresPhysicianSignOff { get; set; }
        public string? RequestedCareLevel { get; set; }
    }

    /// <summary>The confirmation the server returns once the change is requested.</summary>
    public sealed record CareLevelChangeConfirmation(string Confirmation);
}
