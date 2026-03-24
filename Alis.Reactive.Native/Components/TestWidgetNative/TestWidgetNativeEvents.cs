namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on TestWidgetNative.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class TestWidgetNativeEvents
    {
        public static readonly TestWidgetNativeEvents Instance = new TestWidgetNativeEvents();
        private TestWidgetNativeEvents() { }

        /// <summary>Fires when the user changes the input value (DOM "change" event).</summary>
        public TypedEventDescriptor<TestWidgetNativeChangeArgs> Changed =>
            new TypedEventDescriptor<TestWidgetNativeChangeArgs>(
                "change", new TestWidgetNativeChangeArgs());
    }
}
