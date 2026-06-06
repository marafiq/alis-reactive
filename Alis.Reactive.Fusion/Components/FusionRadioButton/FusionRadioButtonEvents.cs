namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionRadioButton"/> component.
    /// </summary>
    public sealed class FusionRadioButtonEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionRadioButtonEvents Instance = new FusionRadioButtonEvents();
        private FusionRadioButtonEvents() { }

        /// <summary>Fires when this radio button becomes selected.</summary>
        public TypedEvent<FusionRadioButtonChangeArgs> Changed =>
            new TypedEvent<FusionRadioButtonChangeArgs>(
                "change", new FusionRadioButtonChangeArgs());
    }
}
