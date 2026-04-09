namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.ActionComplete (SF "actionComplete" event).
    /// Fires after a scheduler action completes successfully.
    /// Verified: sf-schedule-test.html — requestType matches ActionBegin,
    /// addedRecords/changedRecords/deletedRecords contain affected event data.
    /// </summary>
    public class FusionScheduleActionCompleteArgs
    {
        /// <summary>The type of action that completed.</summary>
        public string RequestType { get; set; } = "";

        public FusionScheduleActionCompleteArgs() { }
    }
}
