namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.EventClicked (SF "eventClick" event).
    /// Fires when an appointment/event is clicked.
    /// Verified: sf-schedule-test.html — event data comes via preceding "select" event,
    /// eventClick itself carries element references only.
    /// </summary>
    public class FusionScheduleEventClickArgs
    {
        /// <summary>Set to true to prevent the default quick-info popup.</summary>
        public bool Cancel { get; set; }

        public FusionScheduleEventClickArgs() { }
    }
}
