namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionInputMask.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionInputMaskEvents
    {
        public static readonly FusionInputMaskEvents Instance = new FusionInputMaskEvents();
        private FusionInputMaskEvents() { }

        /// <summary>Fires when the masked value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionInputMaskChangeArgs> Changed =>
            new TypedEventDescriptor<FusionInputMaskChangeArgs>(
                "change", new FusionInputMaskChangeArgs());
    }
}
