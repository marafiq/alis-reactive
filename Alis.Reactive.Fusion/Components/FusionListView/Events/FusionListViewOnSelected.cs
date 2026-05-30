using Alis.Reactive.Builders;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionListView"/> item is selected.
    /// </summary>
    public sealed class FusionListViewSelectArgs
    {
        /// <summary>Gets or sets the selected item's visible text.</summary>
        public string? Text { get; set; }

        /// <summary>Gets or sets the selected item's zero-based index when the event came from user interaction.</summary>
        public int? Index { get; set; }

        /// <summary>Gets or sets whether the selection came from user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>Gets or sets the checkbox state for checkbox ListViews.</summary>
        public bool? IsChecked { get; set; }

        /// <summary>Gets or sets whether Syncfusion should cancel the selection.</summary>
        public bool Cancel { get; set; }

        public FusionListViewSelectArgs() { }
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
