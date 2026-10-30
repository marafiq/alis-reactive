namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>A nurse's medication pass: whether each resident's dose was given, keyed by resident id.</summary>
    public class MedicationPassModel
    {
        public Dictionary<int, bool> Administered { get; set; } = new();
    }

    /// <summary>The pass confirmation the server returns once the doses are recorded, with the next pass's time.</summary>
    public sealed record MedicationPassConfirmation(string Confirmation, string NextPass);

    /// <summary>The pharmacy's answer once it has ordered the refills a pass used up.</summary>
    public sealed record RefillOrder(string Status);
}
