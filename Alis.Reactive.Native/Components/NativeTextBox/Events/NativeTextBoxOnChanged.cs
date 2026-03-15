namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeTextBox.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("hello")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class NativeTextBoxChangeArgs
    {
        /// <summary>The input's value after the change.</summary>
        public string? Value { get; set; }

        public NativeTextBoxChangeArgs() { }
    }
}
