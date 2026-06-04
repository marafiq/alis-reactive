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

        /// <summary>The current editor mode ("Inline" or "Popup").</summary>
        public string? Mode { get; set; }

        /// <summary>The Syncfusion event name.</summary>
        public string? Name { get; set; }
    }

    /// <summary>Typed mutations on the endEdit event args for <see cref="FusionInPlaceEditor"/>.</summary>
    public static class FusionInPlaceEditorEndEditArgsExtensions
    {
        /// <summary>Keeps the editor in edit mode even though a save or cancel was attempted.</summary>
        /// <param name="args">The endEdit event args.</param>
        /// <param name="pipeline">The current pipeline builder.</param>
        public static void PreventDefault(
            this FusionInPlaceEditorEndEditArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(PayloadSource.Event(), "cancel", ValueExpression.Literal(true)));
        }
    }
}
