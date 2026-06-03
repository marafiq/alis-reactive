namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.ActionComplete.
    /// Fires after a scheduler action completes successfully.
    /// </summary>
    public class FusionScheduleActionCompleteArgs
    {
        /// <summary>The type of action that completed.</summary>
        public string RequestType { get; set; } = "";

        public FusionScheduleActionCompleteArgs() { }
    }
}
