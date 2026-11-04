namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
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
