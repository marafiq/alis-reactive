namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeCheckList"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeCheckListEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeCheckListEvents Instance = new NativeCheckListEvents();
        private NativeCheckListEvents() { }

        /// <summary>
        /// Fires when the user checks or unchecks any checkbox in the list.
        /// </summary>
        public TypedEventDescriptor<NativeCheckListChangeArgs> Changed =>
            new TypedEventDescriptor<NativeCheckListChangeArgs>(
                "change", new NativeCheckListChangeArgs());
    }
}
