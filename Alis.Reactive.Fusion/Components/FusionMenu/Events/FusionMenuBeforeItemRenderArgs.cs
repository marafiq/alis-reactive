namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered before a menu item renders.
    /// </summary>
    public sealed class FusionMenuBeforeItemRenderArgs
    {
        public FusionMenuItem Item { get; set; } = new FusionMenuItem();
    }
}
