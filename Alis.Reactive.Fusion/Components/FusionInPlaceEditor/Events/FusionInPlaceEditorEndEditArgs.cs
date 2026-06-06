using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>Event payload delivered when a <see cref="FusionInPlaceEditor"/> leaves edit mode.</summary>
    public class FusionInPlaceEditorEndEditArgs
    {
        /// <summary>What ended the edit: <c>"submit"</c> on save, <c>"cancel"</c> on cancel click.</summary>
        public string? Action { get; set; }

        /// <summary>Whether leaving edit mode is cancelled.</summary>
        public bool Cancel { get; set; }

        /// <summary>Editor display mode for this end-edit event: <c>"Inline"</c> or <c>"Popup"</c>.</summary>
        public string? Mode { get; set; }

        /// <summary>Syncfusion event token exposed as <c>args.name</c>.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Typed event-payload operations for the endEdit event args of <see cref="FusionInPlaceEditor"/>.</summary>
    public static class FusionInPlaceEditorEndEditArgsExtensions
    {
        /// <summary>Keeps the editor in edit mode even though a save or cancel was attempted.</summary>
        /// <param name="pipeline">Pipeline that records the event-arg cancellation mutation.</param>
        public static void PreventDefault(
            this FusionInPlaceEditorEndEditArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
