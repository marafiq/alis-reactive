namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionInputMask"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionInputMaskEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionInputMaskEvents Instance = new FusionInputMaskEvents();
        private FusionInputMaskEvents() { }

        /// <summary>Fires when the masked value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionInputMaskChangeArgs> Changed =>
            new TypedEventDescriptor<FusionInputMaskChangeArgs>(
                "change", new FusionInputMaskChangeArgs());
    }
}
