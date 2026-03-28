namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeCheckBox"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeCheckBoxEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeCheckBoxEvents Instance = new NativeCheckBoxEvents();
        private NativeCheckBoxEvents() { }

        /// <summary>
        /// Fires when the user checks or unchecks the checkbox.
        /// </summary>
        public TypedEventDescriptor<NativeCheckBoxChangeArgs> Changed =>
            new TypedEventDescriptor<NativeCheckBoxChangeArgs>(
                "change", new NativeCheckBoxChangeArgs());
    }
}
