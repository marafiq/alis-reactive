namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeHiddenField"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda. Hidden inputs emit
    /// <c>change</c> only when application code dispatches that DOM event.
    /// </remarks>
    public sealed class NativeHiddenFieldEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeHiddenFieldEvents Instance = new NativeHiddenFieldEvents();
        private NativeHiddenFieldEvents() { }

        /// <summary>
        /// Fires when a DOM <c>change</c> event is dispatched for the hidden input.
        /// </summary>
        public TypedEvent<NativeHiddenFieldChangeArgs> Changed =>
            new TypedEvent<NativeHiddenFieldChangeArgs>(
                "change", new NativeHiddenFieldChangeArgs());
    }
}
