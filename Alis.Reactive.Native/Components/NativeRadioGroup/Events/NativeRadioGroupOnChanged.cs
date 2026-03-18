namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeRadioGroup.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("Memory Care")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class NativeRadioGroupChangeArgs
    {
        /// <summary>The selected radio button's value after the change.</summary>
        public string? Value { get; set; }

        public NativeRadioGroupChangeArgs() { }
    }
}
