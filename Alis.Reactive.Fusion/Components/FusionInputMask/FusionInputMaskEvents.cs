namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionInputMask"/> component.
    /// </summary>
    public sealed class FusionInputMaskEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionInputMaskEvents Instance = new FusionInputMaskEvents();
        private FusionInputMaskEvents() { }

        /// <summary>Fires when the masked value changes.</summary>
        public TypedEvent<FusionInputMaskChangeArgs> Changed =>
            new TypedEvent<FusionInputMaskChangeArgs>(
                "change", new FusionInputMaskChangeArgs());
    }
}
