namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Exposes the events supported by the Fusion Tab component.
    /// </summary>
    public sealed class FusionTabEvents
    {
        /// <summary>
        /// Gets the singleton event catalog for Fusion Tab.
        /// </summary>
        public static readonly FusionTabEvents Instance = new FusionTabEvents();
        private FusionTabEvents() { }

        /// <summary>Fires when a tab is selected (SF "selected" event).</summary>
        public ReactiveEvent<FusionTabSelectedArgs> Selected =>
            new ReactiveEvent<FusionTabSelectedArgs>(
                "selected", new FusionTabSelectedArgs());
    }
}
