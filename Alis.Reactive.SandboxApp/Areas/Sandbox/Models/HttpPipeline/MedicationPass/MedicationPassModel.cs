namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class MedicationPassModel
    {
        public Dictionary<int, bool> Administered { get; set; } = new();
    }

    public sealed record MedicationPassConfirmation(string Confirmation, string NextPass);

    public sealed record RefillOrder(string Status);
}
