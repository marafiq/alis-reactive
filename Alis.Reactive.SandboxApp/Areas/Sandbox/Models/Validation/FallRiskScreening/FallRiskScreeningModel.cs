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
        public bool ObservedWalkingInPerson { get; set; }
    }

    /// <summary>The confirmation the server returns once the screening is saved.</summary>
    public sealed record FallRiskScreeningConfirmation(string Confirmation);
}
