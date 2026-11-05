namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class CareLevelChangeModel
    {
        public int ResidentId { get; set; }
        public bool RequiresPhysicianSignOff { get; set; }
        public string? RequestedCareLevel { get; set; }
    }

    public sealed record CareLevelChangeConfirmation(string Confirmation);
}
