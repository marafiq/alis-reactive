namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeDropDown"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeDropDownEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeDropDownEvents Instance = new NativeDropDownEvents();
        private NativeDropDownEvents() { }

        /// <summary>
        /// Fires when the user selects a different option.
        /// </summary>
        public TypedEventDescriptor<NativeDropDownChangeArgs> Changed =>
            new TypedEventDescriptor<NativeDropDownChangeArgs>(
                "change", new NativeDropDownChangeArgs());
    }
}
