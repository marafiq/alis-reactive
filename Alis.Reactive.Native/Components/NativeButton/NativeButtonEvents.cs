namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeButton.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Click, (args, p) => { ... })
    /// </summary>
    public sealed class NativeButtonEvents
    {
        public static readonly NativeButtonEvents Instance = new NativeButtonEvents();
        private NativeButtonEvents() { }

        /// <summary>Fires when the user clicks the button (DOM "click" event).</summary>
        public TypedEvent<NativeButtonClickArgs> Click =>
            new TypedEvent<NativeButtonClickArgs>(
                "click", new NativeButtonClickArgs());
    }
}
