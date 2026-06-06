namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionCheckBox"/> component.
    /// </summary>
    public sealed class FusionCheckBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionCheckBoxEvents Instance = new FusionCheckBoxEvents();
        private FusionCheckBoxEvents() { }

        /// <summary>Fires when the checkbox state changes.</summary>
        public TypedEvent<FusionCheckBoxChangeArgs> Changed =>
            new TypedEvent<FusionCheckBoxChangeArgs>(
                "change", new FusionCheckBoxChangeArgs());
    }
}
