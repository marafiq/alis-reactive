namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered when a context menu item is selected.
    /// </summary>
    public sealed class FusionContextMenuSelectArgs
    {
        public FusionContextMenuItem Item { get; set; } = new FusionContextMenuItem();
    }
}
