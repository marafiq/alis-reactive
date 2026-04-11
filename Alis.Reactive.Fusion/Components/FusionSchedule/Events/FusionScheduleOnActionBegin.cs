namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.ActionBegin (SF "actionBegin" event).
    /// Fires before every scheduler action. Set cancel to prevent the action.
    /// requestType: "eventCreate", "eventChange", "eventRemove", "dateNavigate", "viewNavigate".
    /// </summary>
    public class FusionScheduleActionBeginArgs
    {
        /// <summary>The type of action being performed.</summary>
        public string RequestType { get; set; } = "";

        /// <summary>Set to true to cancel the action.</summary>
        public bool Cancel { get; set; }

        public FusionScheduleActionBeginArgs() { }
    }
}
