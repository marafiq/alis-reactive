namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeTextAreaEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// The properties are typed markers for event-payload paths; <c>x => x.Value</c>
    /// resolves to <c>evt.value</c> in condition expressions.
    /// </remarks>
    public class NativeTextAreaChangeArgs
    {
        /// <summary>
        /// Gets or sets the value captured from the change event.
        /// </summary>
        public string? Value { get; set; }

        public NativeTextAreaChangeArgs() { }
    }
}
