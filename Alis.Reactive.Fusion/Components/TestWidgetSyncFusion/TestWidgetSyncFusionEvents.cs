namespace Alis.Reactive.Fusion.Components
{
    public sealed class TestWidgetSyncFusionEvents
    {
        public static readonly TestWidgetSyncFusionEvents Instance = new TestWidgetSyncFusionEvents();
        private TestWidgetSyncFusionEvents() { }

        public TypedEvent<TestWidgetSyncFusionChangeArgs> Changed =>
            new TypedEvent<TestWidgetSyncFusionChangeArgs>("change", new TestWidgetSyncFusionChangeArgs());

        public TypedEvent<TestWidgetSyncFusionItemsChangedArgs> ItemsChanged =>
            new TypedEvent<TestWidgetSyncFusionItemsChangedArgs>("items-changed", new TestWidgetSyncFusionItemsChangedArgs());
    }
}
