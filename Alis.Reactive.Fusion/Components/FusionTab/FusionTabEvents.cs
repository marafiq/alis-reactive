namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionTab"/> component.
    /// </summary>
    public sealed class FusionTabEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionTabEvents Instance = new FusionTabEvents();
        private FusionTabEvents() { }

        /// <summary>Fires when a tab is selected.</summary>
        public TypedEvent<FusionTabSelectedArgs> Selected =>
            new TypedEvent<FusionTabSelectedArgs>(
                "selected", new FusionTabSelectedArgs());
    }
}
