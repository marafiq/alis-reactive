namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeRadioGroup"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeRadioGroupEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeRadioGroupEvents Instance = new NativeRadioGroupEvents();
        private NativeRadioGroupEvents() { }

        /// <summary>
        /// Fires when the user selects a different radio option.
        /// </summary>
        public TypedEventDescriptor<NativeRadioGroupChangeArgs> Changed =>
            new TypedEventDescriptor<NativeRadioGroupChangeArgs>(
                "change", new NativeRadioGroupChangeArgs());
    }
}
