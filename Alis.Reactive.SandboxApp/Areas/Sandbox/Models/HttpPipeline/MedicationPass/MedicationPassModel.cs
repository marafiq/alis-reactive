namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A nurse's medication pass: whether each resident's dose was given, keyed by resident id.</summary>
    public class MedicationPassModel
    {
        public Dictionary<int, bool> Administered { get; set; } = new();
    }

    /// <summary>The pass confirmation the server returns once the doses are recorded.</summary>
    public sealed record MedicationPassConfirmation(string Confirmation);
}
