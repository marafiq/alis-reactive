namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed events exposed by <see cref="NativeHiddenField"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda. Hidden inputs emit
    /// <c>change</c> only when application code dispatches that DOM event.
    /// </remarks>
    public sealed class NativeHiddenFieldEvents
    {
        /// <summary>
        /// Selector instance for <c>.Reactive()</c> event lambdas.
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
