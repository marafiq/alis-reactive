namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FallRiskScreeningModel
    {
        public bool? FellInLast90Days { get; set; }

        public int FallsCount { get; set; }

        public bool InjuryReported { get; set; }

        public List<string> FallFactors { get; set; } = new();

        public string? NeuroChecksStartedAt { get; set; }
        public bool ObservedWalkingInPerson { get; set; }
    }

    public sealed record FallRiskScreeningConfirmation(string Confirmation);
}
