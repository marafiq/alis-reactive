using Alis.Reactive;

namespace Alis.Reactive.Fusion.Components
{
    public sealed class TestWidgetSyncFusionEvents
    {
        public static readonly TestWidgetSyncFusionEvents Instance = new TestWidgetSyncFusionEvents();
        private TestWidgetSyncFusionEvents() { }

        public TypedEventDescriptor<TestWidgetSyncFusionChangeArgs> Changed =>
            new TypedEventDescriptor<TestWidgetSyncFusionChangeArgs>("change", new TestWidgetSyncFusionChangeArgs());

        public TypedEventDescriptor<TestWidgetSyncFusionItemsChangedArgs> ItemsChanged =>
            new TypedEventDescriptor<TestWidgetSyncFusionItemsChangedArgs>("items-changed", new TestWidgetSyncFusionItemsChangedArgs());
    }
}
