namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload delivered when a menu item is selected.
    /// </summary>
    public sealed class FusionMenuSelectArgs
    {
        public FusionMenuItem Item { get; set; } = new FusionMenuItem();
    }
}
