namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionListView"/> component.
    /// </summary>
    public sealed class FusionListViewEvents
    {
        public static readonly FusionListViewEvents Instance = new FusionListViewEvents();
        private FusionListViewEvents() { }

        /// <summary>Fires when a ListView item is selected (SF "select" event).</summary>
        public TypedEvent<FusionListViewSelectArgs> Selected =>
            new TypedEvent<FusionListViewSelectArgs>(
                "select", new FusionListViewSelectArgs());
    }
}
