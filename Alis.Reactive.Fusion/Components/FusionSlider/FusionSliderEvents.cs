namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionSlider"/> component.
    /// </summary>
    public sealed class FusionSliderEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionSliderEvents Instance = new FusionSliderEvents();
        private FusionSliderEvents() { }

        /// <summary>Fires when the scalar slider value changes (SF "change" event).</summary>
        public TypedEvent<FusionSliderChangeArgs> Change =>
            new TypedEvent<FusionSliderChangeArgs>(
                "change", new FusionSliderChangeArgs());

        /// <summary>Fires after the scalar slider value changes (SF "changed" event).</summary>
        public TypedEvent<FusionSliderChangeArgs> Changed =>
            new TypedEvent<FusionSliderChangeArgs>(
                "changed", new FusionSliderChangeArgs());
    }
}
