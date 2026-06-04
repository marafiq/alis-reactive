namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionDropDownButton"/> action item is selected.
    /// </summary>
    public class FusionDropDownButtonSelectArgs
    {
        /// <summary>Gets or sets the selected item metadata from the Syncfusion select event.</summary>
        public FusionDropDownButtonItem Item { get; set; } = new FusionDropDownButtonItem();
    }

    /// <summary>
    /// Narrowed action item payload proven from Syncfusion DropDownButton MenuEventArgs.
    /// </summary>
    public sealed class FusionDropDownButtonItem
    {
        /// <summary>Gets or sets the selected item's Syncfusion id.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Gets or sets the selected item's display text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Gets or sets whether the selected item is disabled.</summary>
        public bool Disabled { get; set; }
    }
}
