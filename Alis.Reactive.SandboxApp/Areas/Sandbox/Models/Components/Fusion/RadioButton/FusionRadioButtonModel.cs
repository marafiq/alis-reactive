namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class FusionRadioButtonModel
    {
        public string RoomChoice { get; set; } = string.Empty;
    }

    public sealed class RoomPlanRequest
    {
        public string Room { get; set; } = string.Empty;

        public bool CompanionSuiteChosen { get; set; }

        public bool CompanionSuiteUnavailable { get; set; }
    }

    public sealed class RoomPlanResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}
