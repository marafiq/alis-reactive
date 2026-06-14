namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Move-in Room and Care Plan intake: the room a coordinator confirms for a
    /// new resident. The selection lives in the rendered radio group; this model
    /// carries the property the field binds to.
    /// </summary>
    public sealed class FusionRadioButtonModel
    {
        public string RoomChoice { get; set; } = string.Empty;
    }

    /// <summary>Room and care confirmation the coordinator submits to the move-in desk.</summary>
    public sealed class RoomPlanRequest
    {
        public string Room { get; set; } = string.Empty;

        public bool CompanionSuiteChosen { get; set; }

        public bool CompanionSuiteUnavailable { get; set; }
    }

    /// <summary>What the move-in desk confirms back to the coordinator.</summary>
    public sealed class RoomPlanResponse
    {
        public string Confirmation { get; set; } = string.Empty;
    }
}
