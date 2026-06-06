namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event args for the Syncfusion Grid "toolbarClick" event.
    /// Fires when any toolbar item is clicked, including custom items.
    /// Read <see cref="Item"/> to branch on which button was pressed.
    /// </summary>
    public class FusionGridToolbarClickArgs
    {
        /// <summary>Toolbar item that raised the click event.</summary>
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
        /// <summary>Toolbar item id, such as a custom command id or <c>{gridId}_excelexport</c>.</summary>
        public string? Id { get; set; }

        /// <summary>Toolbar item display text.</summary>
        public string? Text { get; set; }
    }
}
