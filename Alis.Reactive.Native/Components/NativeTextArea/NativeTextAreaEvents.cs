namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed reactive events for <see cref="NativeTextArea"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeTextAreaEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeTextAreaEvents Instance = new NativeTextAreaEvents();
        private NativeTextAreaEvents() { }

        /// <summary>
        /// Fires when the user changes the textarea value and leaves the field.
        /// </summary>
        public ReactiveEvent<NativeTextAreaChangeArgs> Changed =>
            new ReactiveEvent<NativeTextAreaChangeArgs>(
                "change", new NativeTextAreaChangeArgs());
    }
}
