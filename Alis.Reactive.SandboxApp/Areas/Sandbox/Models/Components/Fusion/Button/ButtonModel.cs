namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Daily Wellness Check-In journey: a care-team member opens the check-in for the
    /// resident assigned to them, prepares the action, then records the visit.
    /// </summary>
    public sealed class ButtonModel
    {
        /// <summary>The resident this check-in belongs to, carried into the action label.</summary>
        public string ResidentName { get; set; } = "Eleanor Whitfield";
    }

    /// <summary>
    /// The check-in summary the action button's runtime state is gathered into when the
    /// care-team member records the visit.
    /// </summary>
    public sealed class ButtonCheckInRequest
    {
        public string Action { get; set; } = string.Empty;

        public bool Locked { get; set; }

        public string Priority { get; set; } = string.Empty;

        public bool Recommended { get; set; }

        public bool FollowUp { get; set; }
    }

    /// <summary>The server's confirmation of a recorded daily wellness check-in.</summary>
    public sealed class ButtonCheckInResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}
