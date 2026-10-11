namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// The review a coordinator completes before a resident moves in: the fall-risk score decides how
    /// much the mobility plan must say, and a returning resident's previous room is checked by the
    /// server against the community's records.
    /// </summary>
    public class MoveInReviewModel
    {
        public string? ResidentName { get; set; }
        public int? FallRiskScore { get; set; }
        public string? MobilityNotes { get; set; }
        public string? FamilyPortalLink { get; set; }
        public bool IsReturning { get; set; }
        public string? PreviousRoom { get; set; }

        /// <summary>The coordinator on duty; the server fills it in, so the page never renders it.</summary>
        public string? ReviewedBy { get; set; }
    }

    /// <summary>What the server returns once the review is saved.</summary>
    public sealed record MoveInReviewSaved(string Confirmation);
}
