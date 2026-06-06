namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeCheckBoxEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Use <c>x =&gt; x.Checked</c> in typed event conditions; the Reactive Plan
    /// reads the checked state from <c>evt.checked</c>.
    /// </remarks>
    public class NativeCheckBoxChangeArgs
    {
        /// <summary>
        /// Checked state after the change event.
        /// </summary>
        public bool? Checked { get; set; }
    }
}
