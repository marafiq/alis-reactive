namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeHiddenField.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("RES-1042")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class NativeHiddenFieldChangeArgs
    {
        /// <summary>The hidden input's value after the change.</summary>
        public string? Value { get; set; }

        public NativeHiddenFieldChangeArgs() { }
    }
}
