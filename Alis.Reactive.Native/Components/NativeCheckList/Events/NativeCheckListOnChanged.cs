namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeCheckListEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// The properties are typed markers for event-payload paths; <c>x => x.Value</c>
    /// resolves to <c>evt.value</c> in condition expressions.
    /// </remarks>
    public class NativeCheckListChangeArgs
    {
        /// <summary>
        /// Gets or sets the checked values after the change.
        /// </summary>
        public string[]? Value { get; set; }

        public NativeCheckListChangeArgs() { }
    }
}
