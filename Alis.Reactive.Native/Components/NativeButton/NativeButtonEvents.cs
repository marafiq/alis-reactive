namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed events exposed by <see cref="NativeButton"/>.
    /// </summary>
    public sealed class NativeButtonEvents
    {
        /// <summary>
        /// Selector instance for <c>.Reactive()</c> event lambdas.
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
