namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered before a context menu item renders.
    /// </summary>
    public sealed class FusionContextMenuBeforeItemRenderArgs
    {
        public FusionContextMenuItem Item { get; set; } = new FusionContextMenuItem();
    }
}
