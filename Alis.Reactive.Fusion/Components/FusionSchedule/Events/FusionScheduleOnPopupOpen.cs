using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSchedule.PopupOpen (SF "popupOpen" event).
    /// Fires before any popup opens. Set cancel to prevent.
    /// Verified: sf-schedule-test.html — type values:
    /// "QuickInfo" (click tooltip), "Editor" (full edit modal), "DeleteAlert" (confirmation).
    /// </summary>
    public class FusionSchedulePopupOpenArgs
    {
        /// <summary>The popup type: "QuickInfo", "Editor", or "DeleteAlert".</summary>
        public string Type { get; set; } = "";

        /// <summary>Set to true to prevent the popup from opening.</summary>
        public bool Cancel { get; set; }

        /// <summary>The event data associated with this popup (id, subject, startTime, etc.).</summary>
        public FusionSchedulePopupData Data { get; set; } = new FusionSchedulePopupData();

        public FusionSchedulePopupOpenArgs() { }
    }

    /// <summary>
    /// Event data nested inside popupOpen args. Contains the schedule event being
    /// acted on. Use with FromEvent to pass the event ID to server endpoints.
    /// </summary>
    public class FusionSchedulePopupData
    {
        /// <summary>The event ID.</summary>
        public int Id { get; set; }

        /// <summary>The event subject/title.</summary>
        public string Subject { get; set; } = "";

        public FusionSchedulePopupData() { }
    }

    /// <summary>
    /// Extensions for <see cref="FusionSchedulePopupOpenArgs"/> — cancel SF popup to use custom UI.
    /// Same pattern as <see cref="FusionAutoCompleteFilteringArgs"/>.PreventDefault.
    /// </summary>
    public static class FusionSchedulePopupOpenArgsExtensions
    {
        /// <summary>
        /// Cancels the SF popup from opening. Use inside a When(args.Type).Eq("Editor") branch
        /// to replace the built-in editor with a custom form loaded via Into().
        /// Runtime: sets evt.cancel = true on the SF popupOpen event args.
        /// </summary>
        public static void PreventDefault(
            this FusionSchedulePopupOpenArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(Reaction.Set(PayloadSource.Event(), "cancel", ValueProducer.Literal(true)));
        }
    }
}
