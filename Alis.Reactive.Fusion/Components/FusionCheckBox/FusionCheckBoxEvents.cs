namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionCheckBox"/> component.
    /// </summary>
    public sealed class FusionCheckBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionCheckBoxEvents Instance = new FusionCheckBoxEvents();
        private FusionCheckBoxEvents() { }

        /// <summary>Fires when the checkbox state changes (SF "change" event).</summary>
        public TypedEvent<FusionCheckBoxChangeArgs> Changed =>
            new TypedEvent<FusionCheckBoxChangeArgs>(
                "change", new FusionCheckBoxChangeArgs());
    }
}
