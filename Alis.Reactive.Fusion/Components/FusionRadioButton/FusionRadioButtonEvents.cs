namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionRadioButton"/> component.
    /// </summary>
    public sealed class FusionRadioButtonEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionRadioButtonEvents Instance = new FusionRadioButtonEvents();
        private FusionRadioButtonEvents() { }

        /// <summary>Fires when this radio button becomes selected.</summary>
        public TypedEvent<FusionRadioButtonChangeArgs> Changed =>
            new TypedEvent<FusionRadioButtonChangeArgs>(
                "change", new FusionRadioButtonChangeArgs());
    }
}
