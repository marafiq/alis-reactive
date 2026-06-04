namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeRadioGroupEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// The properties are typed markers for event-payload paths; <c>x => x.Value</c>
    /// resolves to <c>evt.value</c> in condition expressions.
    /// </remarks>
    public class NativeRadioGroupChangeArgs
    {
        /// <summary>
        /// Gets or sets the selected radio button's value after the change.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Initializes a new instance for event payload binding.
        /// </summary>
        public NativeRadioGroupChangeArgs() { }
    }
}
