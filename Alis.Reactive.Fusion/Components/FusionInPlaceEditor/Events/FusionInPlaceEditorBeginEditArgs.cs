using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>Event payload delivered before a <see cref="FusionInPlaceEditor"/> enters edit mode.</summary>
    public class FusionInPlaceEditorBeginEditArgs
    {
        /// <summary>Whether opening the editor is cancelled.</summary>
        public bool Cancel { get; set; }

        /// <summary>Whether focusing the inner input is cancelled.</summary>
        public bool CancelFocus { get; set; }

        /// <summary>Editor display mode for this begin-edit event: <c>"Inline"</c> or <c>"Popup"</c>.</summary>
        public string? Mode { get; set; }

        /// <summary>Syncfusion event token exposed as <c>args.name</c>.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Typed event-payload operations for the beginEdit event args of <see cref="FusionInPlaceEditor"/>.</summary>
    public static class FusionInPlaceEditorBeginEditArgsExtensions
    {
        /// <summary>Cancels entering edit mode. The pencil click is ignored.</summary>
        /// <param name="pipeline">Pipeline that records the event-arg cancellation mutation.</param>
        public static void PreventDefault(
            this FusionInPlaceEditorBeginEditArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
