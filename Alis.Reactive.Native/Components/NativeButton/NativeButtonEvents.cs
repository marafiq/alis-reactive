namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeButton"/>.
    /// </summary>
    public sealed class NativeButtonEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeButtonEvents Instance = new NativeButtonEvents();
        private NativeButtonEvents() { }

        /// <summary>
        /// Fires when the user clicks the button.
        /// </summary>
        public TypedEvent<NativeButtonClickArgs> Click =>
            new TypedEvent<NativeButtonClickArgs>(
                "click", new NativeButtonClickArgs());
    }
}
