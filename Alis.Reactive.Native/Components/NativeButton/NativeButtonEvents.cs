namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Exposes the reactive events available on <see cref="NativeButton"/>.
    /// </summary>
    public sealed class NativeButtonEvents
    {
        private static readonly EventContractAuthoring ClickContract =
            EventPayloadContractAuthoring.Define<NativeButtonClickArgs>(_ => { });

        /// <summary>Gets the singleton event surface instance.</summary>
        public static readonly NativeButtonEvents Instance = new NativeButtonEvents();
        private NativeButtonEvents() { }

        /// <summary>Gets the button click event.</summary>
        public ReactiveEvent<NativeButtonClickArgs> Click =>
            new ReactiveEvent<NativeButtonClickArgs>(
                "click", ClickContract);
    }
}
