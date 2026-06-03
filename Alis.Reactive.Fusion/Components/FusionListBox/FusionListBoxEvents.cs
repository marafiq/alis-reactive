namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionListBox"/> component.
    /// </summary>
    public sealed class FusionListBoxEvents
    {
        public static readonly FusionListBoxEvents Instance = new FusionListBoxEvents();
        private FusionListBoxEvents() { }

        /// <summary>Fires when selected values change.</summary>
        public TypedEvent<FusionListBoxChangeArgs> Changed =>
            new TypedEvent<FusionListBoxChangeArgs>(
                "change", new FusionListBoxChangeArgs());
    }
}
