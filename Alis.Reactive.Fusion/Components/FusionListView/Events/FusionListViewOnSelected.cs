using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionListView"/> item is selected.
    /// </summary>
    public sealed class FusionListViewSelectArgs
    {
        /// <summary>Selected item's visible text.</summary>
        public string? Text { get; set; }

        /// <summary>Selected item's zero-based index when the event came from user interaction.</summary>
        public int? Index { get; set; }

        /// <summary>Whether the selection came from user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>Checkbox state for checkbox ListViews.</summary>
        public bool? IsChecked { get; set; }

        /// <summary>Whether Syncfusion should cancel the selection.</summary>
        public bool Cancel { get; set; }
    }

    public static class FusionListViewSelectArgsExtensions
    {
        /// <summary>Cancels Syncfusion's selected item commit for the current select event.</summary>
        public static void CancelSelection(
            this FusionListViewSelectArgs args,
            IReactionEmitter pipeline)
        {
            pipeline.AddStep(ReactionGraph.Set(
                PayloadSource.Event(),
                "cancel",
                ValueExpression.Literal(true)));
        }
    }
}
