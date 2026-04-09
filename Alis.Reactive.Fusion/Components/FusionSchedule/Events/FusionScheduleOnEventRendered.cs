namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.EventRendered (SF "eventRendered" event).
    /// Fires before each event/appointment renders on the UI.
    /// Use to apply custom styling (e.g., unassigned shifts in red).
    /// Set cancel to true to prevent the event from rendering.
    /// </summary>
    public class FusionScheduleEventRenderedArgs
    {
        /// <summary>Set to true to prevent this event from rendering.</summary>
        public bool Cancel { get; set; }

        public FusionScheduleEventRenderedArgs() { }
    }
}
