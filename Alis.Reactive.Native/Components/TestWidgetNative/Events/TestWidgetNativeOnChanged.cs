namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Represents the payload surface for the native test-widget change event.
    /// </summary>
    public class TestWidgetNativeChangeArgs
    {
        /// <summary>Gets or sets the widget value after the change.</summary>
        public string? Value { get; set; }

        /// <summary>Creates an empty widget change payload marker.</summary>
        internal TestWidgetNativeChangeArgs() { }
    }
}
