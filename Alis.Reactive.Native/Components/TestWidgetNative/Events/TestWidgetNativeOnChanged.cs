namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for TestWidgetNative.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based source binding:
    ///   p.Element("result").SetText(args, x => x.Value)
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class TestWidgetNativeChangeArgs
    {
        /// <summary>The input's value after the change.</summary>
        public string? Value { get; set; }

        public TestWidgetNativeChangeArgs() { }
    }
}
