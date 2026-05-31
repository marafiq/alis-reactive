namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a Toolbar item is clicked.
    /// </summary>
    public sealed class FusionToolbarClickedArgs
    {
        /// <summary>Toolbar item metadata from the Syncfusion click event.</summary>
        public FusionToolbarItem Item { get; set; } = new FusionToolbarItem();
    }

    /// <summary>
    /// Narrowed toolbar item payload proven from Syncfusion toolbar click behavior.
    /// </summary>
    public sealed class FusionToolbarItem
    {
        /// <summary>Toolbar item id.</summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>Toolbar item text.</summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>Whether the clicked item is disabled.</summary>
        public bool Disabled { get; set; }
    }
}
