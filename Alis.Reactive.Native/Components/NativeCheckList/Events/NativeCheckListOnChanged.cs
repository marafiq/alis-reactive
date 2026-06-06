namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeCheckListEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Use <c>x =&gt; x.Value</c> in typed event conditions; the Reactive Plan
    /// reads the checked values from <c>evt.value</c>.
    /// </remarks>
    public class NativeCheckListChangeArgs
    {
        /// <summary>
        /// Checked values after the change event.
        /// </summary>
        public string[]? Value { get; set; }
    }
}
