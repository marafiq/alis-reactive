namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Exposes the events supported by the Syncfusion test widget.
    /// </summary>
    public sealed class TestWidgetSyncFusionEvents
    {
        /// <summary>
        /// Gets the singleton event catalog for the Syncfusion test widget.
        /// </summary>
        public static readonly TestWidgetSyncFusionEvents Instance = new TestWidgetSyncFusionEvents();
        private TestWidgetSyncFusionEvents() { }

        /// <summary>
        /// Gets the event raised when the widget value changes.
        /// </summary>
        public ReactiveEvent<TestWidgetSyncFusionChangeArgs> Changed =>
            new ReactiveEvent<TestWidgetSyncFusionChangeArgs>("change", new TestWidgetSyncFusionChangeArgs());

        /// <summary>
        /// Gets the event raised when the widget items collection changes.
        /// </summary>
        public ReactiveEvent<TestWidgetSyncFusionItemsChangedArgs> ItemsChanged =>
            new ReactiveEvent<TestWidgetSyncFusionItemsChangedArgs>("items-changed", new TestWidgetSyncFusionItemsChangedArgs());
    }
}
