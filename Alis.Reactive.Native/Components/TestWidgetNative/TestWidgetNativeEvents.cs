namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Exposes the reactive events available on <see cref="TestWidgetNative"/>.
    /// </summary>
    public sealed class TestWidgetNativeEvents
    {
        /// <summary>Gets the singleton event surface instance.</summary>
        public static readonly TestWidgetNativeEvents Instance = new TestWidgetNativeEvents();
        private TestWidgetNativeEvents() { }

        /// <summary>Gets the widget change event.</summary>
        public ReactiveEvent<TestWidgetNativeChangeArgs> Changed =>
            new ReactiveEvent<TestWidgetNativeChangeArgs>(
                "change", new TestWidgetNativeChangeArgs());
    }
}
