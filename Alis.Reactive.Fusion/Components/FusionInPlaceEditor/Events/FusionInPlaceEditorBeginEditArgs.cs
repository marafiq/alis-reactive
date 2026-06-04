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

        /// <summary>The current editor mode ("Inline" or "Popup").</summary>
        public string? Mode { get; set; }

        /// <summary>The Syncfusion event name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Typed mutations on the beginEdit event args for <see cref="FusionInPlaceEditor"/>.</summary>
    public static class FusionInPlaceEditorBeginEditArgsExtensions
    {
        /// <summary>Cancels entering edit mode. The pencil click is ignored.</summary>
        /// <param name="args">The beginEdit event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        public static void PreventDefault(
            this FusionInPlaceEditorBeginEditArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
