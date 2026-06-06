namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionInputMask"/> component.
    /// </summary>
    public sealed class FusionInputMaskEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionInputMaskEvents Instance = new FusionInputMaskEvents();
        private FusionInputMaskEvents() { }

        /// <summary>Fires when the masked value changes.</summary>
        public TypedEvent<FusionInputMaskChangeArgs> Changed =>
            new TypedEvent<FusionInputMaskChangeArgs>(
                "change", new FusionInputMaskChangeArgs());
    }
}
