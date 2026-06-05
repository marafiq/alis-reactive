namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionSplitButton"/> secondary action item is selected.
    /// </summary>
    public class FusionSplitButtonSelectArgs
    {
        /// <summary>Selected item metadata from the Syncfusion select event.</summary>
        public FusionSplitButtonItem Item { get; set; } = new FusionSplitButtonItem();
    }

    /// <summary>
    /// Narrowed action item payload proven from Syncfusion SplitButton MenuEventArgs.
    /// </summary>
    public sealed class FusionSplitButtonItem
    {
        /// <summary>Selected item's Syncfusion id.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Selected item's display text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Whether the selected item is disabled.</summary>
        public bool Disabled { get; set; }
    }
}
