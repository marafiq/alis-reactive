namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for the test widget items-changed event.
    /// </summary>
    public class TestWidgetSyncFusionItemsChangedArgs
    {
        /// <summary>Gets or sets the number of items currently held by the widget.</summary>
        public int Count { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestWidgetSyncFusionItemsChangedArgs"/> class.
        /// </summary>
        public TestWidgetSyncFusionItemsChangedArgs() { }
    }
}
