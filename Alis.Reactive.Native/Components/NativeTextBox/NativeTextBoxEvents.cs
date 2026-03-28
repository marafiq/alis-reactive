namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeTextBox"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeTextBoxEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeTextBoxEvents Instance = new NativeTextBoxEvents();
        private NativeTextBoxEvents() { }

        /// <summary>
        /// Fires when the user changes the input value and leaves the field.
        /// </summary>
        public TypedEventDescriptor<NativeTextBoxChangeArgs> Changed =>
            new TypedEventDescriptor<NativeTextBoxChangeArgs>(
                "change", new NativeTextBoxChangeArgs());
    }
}
