namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeRadioGroupEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// Use <c>x =&gt; x.Value</c> in typed event conditions; the Reactive Plan
    /// reads the value from <c>evt.value</c>.
    /// </remarks>
    public class NativeRadioGroupChangeArgs
    {
        /// <summary>
        /// Selected radio button value after the change event.
        /// </summary>
        public string? Value { get; set; }
    }
}
