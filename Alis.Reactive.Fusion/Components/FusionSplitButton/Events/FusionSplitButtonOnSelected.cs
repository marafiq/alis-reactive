namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionSplitButton"/> secondary action item is selected.
    /// </summary>
    public class FusionSplitButtonSelectArgs
    {
        /// <summary>Gets or sets the selected item metadata from the Syncfusion select event.</summary>
        public FusionSplitButtonItem Item { get; set; } = new FusionSplitButtonItem();
        public FusionSplitButtonSelectArgs() { }
    }

    /// <summary>
    /// Narrowed action item payload proven from Syncfusion SplitButton MenuEventArgs.
    /// </summary>
    public sealed class FusionSplitButtonItem
    {
        /// <summary>Gets or sets the selected item's Syncfusion id.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Gets or sets the selected item's display text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Gets or sets whether the selected item is disabled.</summary>
        public bool Disabled { get; set; }
    }
}
