namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for the test widget change event.
    /// </summary>
    public class TestWidgetSyncFusionChangeArgs
    {
        /// <summary>Gets or sets the new widget value.</summary>
        public string? NewValue { get; set; }

        /// <summary>Gets or sets the previous widget value.</summary>
        public string? PreviousValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestWidgetSyncFusionChangeArgs"/> class.
        /// </summary>
        public TestWidgetSyncFusionChangeArgs() { }
    }
}
