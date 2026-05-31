namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionToolbar"/> component.
    /// </summary>
    public sealed class FusionToolbarEvents
    {
        public static readonly FusionToolbarEvents Instance = new FusionToolbarEvents();

        private FusionToolbarEvents()
        {
        }

        /// <summary>Fires when a toolbar item is clicked.</summary>
        public TypedEvent<FusionToolbarClickedArgs> Clicked =>
            new TypedEvent<FusionToolbarClickedArgs>("clicked", new FusionToolbarClickedArgs());
    }
}
