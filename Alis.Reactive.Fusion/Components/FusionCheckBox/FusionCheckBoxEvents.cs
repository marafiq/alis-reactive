namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionCheckBox"/> component.
    /// </summary>
    public sealed class FusionCheckBoxEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionCheckBoxEvents Instance = new FusionCheckBoxEvents();
        private FusionCheckBoxEvents() { }

        /// <summary>Fires when checkbox state changes.</summary>
        public TypedEvent<FusionCheckBoxChangeArgs> Changed =>
            new TypedEvent<FusionCheckBoxChangeArgs>(
                "change", new FusionCheckBoxChangeArgs());
    }
}
