using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered before Syncfusion Schedule opens a popup.
    /// Set <see cref="Cancel"/> to prevent the popup.
    /// </summary>
    public class FusionSchedulePopupOpenArgs
    {
        /// <summary>Popup type, for example <c>QuickInfo</c> or <c>Editor</c>.</summary>
        public string Type { get; set; } = "";

        /// <summary>Set to true before the callback returns to prevent the popup from opening.</summary>
        public bool Cancel { get; set; }

        /// <summary>Schedule event data associated with this popup when Syncfusion provides it.</summary>
        public FusionSchedulePopupData Data { get; set; } = new FusionSchedulePopupData();
    }

    /// <summary>
    /// Schedule event subset available while a Syncfusion Schedule popup opens.
    /// </summary>
    public class FusionSchedulePopupData
    {
        /// <summary>Schedule event identifier.</summary>
        public int Id { get; set; }

        /// <summary>Display title for the appointment.</summary>
        public string Subject { get; set; } = "";
    }

    /// <summary>
    /// Typed event-payload operations for Syncfusion Schedule <c>popupOpen</c> event args.
    /// </summary>
    public static class FusionSchedulePopupOpenArgsExtensions
    {
        /// <summary>
        /// Prevents the current Syncfusion popup from opening.
        /// Use inside a branch such as <c>When(args.Type).Eq("Editor")</c> when
        /// replacing the built-in editor with a custom form. This sets the event
        /// payload's <c>cancel</c> member before Syncfusion resumes its popup lifecycle.
        /// </summary>
        public static void PreventDefault(
            this FusionSchedulePopupOpenArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
