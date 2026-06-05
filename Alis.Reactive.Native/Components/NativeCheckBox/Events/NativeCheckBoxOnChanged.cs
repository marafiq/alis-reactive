namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Event args for <see cref="NativeCheckBoxEvents.Changed"/>.
    /// </summary>
    /// <remarks>
    /// The properties are typed markers for event-payload paths; <c>x => x.Checked</c>
    /// resolves to <c>evt.checked</c> in condition expressions.
    /// </remarks>
    public class NativeCheckBoxChangeArgs
    {
        /// <summary>
        /// Checked state after the change event.
        /// </summary>
        public bool? Checked { get; set; }
    }
}
