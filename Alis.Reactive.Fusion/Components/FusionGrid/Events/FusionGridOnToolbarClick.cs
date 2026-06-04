namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the Syncfusion Grid "toolbarClick" event.
    /// Fires when any toolbar item is clicked, including custom items.
    /// Read <see cref="Item"/> to branch on which button was pressed.
    /// </summary>
    public class FusionGridToolbarClickArgs
    {
        /// <summary>The clicked toolbar item.</summary>
        public FusionGridToolbarItem Item { get; set; } = new FusionGridToolbarItem();

        /// <summary>Set true to cancel the default toolbar action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>
    /// The toolbar item carried by a Grid "toolbarClick" event.
    /// Custom items expose the id and text declared on the builder Toolbar.
    /// </summary>
    public class FusionGridToolbarItem
    {
        /// <summary>The toolbar item id (e.g. a custom command id, or "{gridId}_excelexport").</summary>
        public string? Id { get; set; }

        /// <summary>The toolbar item display text.</summary>
        public string? Text { get; set; }
    }
}
