namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // Journey: a new resident completes their Move-In Services Agreement with a
    // move-in coordinator. Accepting the residency agreement unlocks the optional
    // services; the coordinator can pre-select a recommended service, mark one for
    // follow-up when the resident is undecided, or toggle one on the resident's behalf.
    public sealed class FusionCheckBoxModel
    {
        public bool AgreementAccepted { get; set; }

        public bool WeeklyHousekeeping { get; set; }
    }

    public sealed class MoveInAgreementRequest
    {
        public bool AgreementAccepted { get; set; }

        public bool WeeklyHousekeeping { get; set; }

        public bool HousekeepingNeedsFollowUp { get; set; }
    }

    public sealed class MoveInAgreementResponse
    {
        public string Summary { get; set; } = string.Empty;
    }
}
