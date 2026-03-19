namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeTextArea.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("hello")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class NativeTextAreaChangeArgs
    {
        /// <summary>The textarea's value after the change.</summary>
        public string? Value { get; set; }

        public NativeTextAreaChangeArgs() { }
    }
}
