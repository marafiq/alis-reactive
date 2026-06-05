namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeHiddenFieldEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// The properties are typed markers for event-payload paths; <c>x => x.Value</c>
    /// resolves to <c>evt.value</c> in condition expressions.
    /// </remarks>
    public class NativeHiddenFieldChangeArgs
    {
        /// <summary>
        /// Hidden input value after the change event.
        /// </summary>
        public string? Value { get; set; }
    }
}
