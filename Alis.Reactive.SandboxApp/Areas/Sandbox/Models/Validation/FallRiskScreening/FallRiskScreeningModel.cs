namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A nurse's fall-risk screening for one resident. Whether the resident fell in the last
    /// 90 days is a yes/no question that must be answered: nullable, because unanswered is not
    /// "no". The in-person observation is a plain attestation the nurse must tick.
    /// </summary>
    public class FallRiskScreeningModel
    {
        public bool? FellInLast90Days { get; set; }

        /// <summary>How many times the resident fell in the last 90 days; 0 when they did not fall.</summary>
        public int FallsCount { get; set; }

        /// <summary>Whether a fall in the last 90 days caused an injury.</summary>
        public bool InjuryReported { get; set; }

        /// <summary>What contributed to the falls, as the nurse ticks it.</summary>
        public List<string> FallFactors { get; set; } = new();

        /// <summary>When neurological checks started after an unwitnessed fall.</summary>
        public string? NeuroChecksStartedAt { get; set; }
        public bool ObservedWalkingInPerson { get; set; }
    }

    /// <summary>The confirmation the server returns once the screening is saved.</summary>
    public sealed record FallRiskScreeningConfirmation(string Confirmation);
}
