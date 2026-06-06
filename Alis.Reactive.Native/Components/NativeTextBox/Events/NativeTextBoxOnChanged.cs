namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeTextBoxEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Use <c>x =&gt; x.Value</c> in typed event conditions; the Reactive Plan
    /// reads the value from <c>evt.value</c>.
    /// </remarks>
    public class NativeTextBoxChangeArgs
    {
        /// <summary>
        /// Text input value captured from the change event.
        /// </summary>
        public string? Value { get; set; }
    }
}
