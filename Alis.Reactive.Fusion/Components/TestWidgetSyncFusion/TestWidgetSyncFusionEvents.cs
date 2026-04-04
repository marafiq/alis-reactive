using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Exposes the events supported by the Syncfusion test widget.
    /// </summary>
    public sealed class TestWidgetSyncFusionEvents
    {
        private static readonly CapabilityProperty NewValueEventMember = CapabilityProperty.Named("newValue");
        private static readonly CapabilityProperty PreviousValueEventMember = CapabilityProperty.Named("previousValue");
        private static readonly CapabilityProperty CountEventMember = CapabilityProperty.Named("count");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<TestWidgetSyncFusionChangeArgs>(payload =>
            {
                payload.Read(args => args.NewValue, NewValueEventMember);
                payload.Read(args => args.PreviousValue, PreviousValueEventMember);
            });

        private static readonly EventContractAuthoring ItemsChangedContract =
            EventPayloadContractAuthoring.Define<TestWidgetSyncFusionItemsChangedArgs>(payload =>
            {
                payload.Read(args => args.Count, CountEventMember);
            });

        /// <summary>
        /// Gets the singleton event catalog for the Syncfusion test widget.
        /// </summary>
        public static readonly TestWidgetSyncFusionEvents Instance = new TestWidgetSyncFusionEvents();
        private TestWidgetSyncFusionEvents() { }

        /// <summary>
        /// Gets the event raised when the widget value changes.
        /// </summary>
        public ReactiveEvent<TestWidgetSyncFusionChangeArgs> Changed =>
            new ReactiveEvent<TestWidgetSyncFusionChangeArgs>(
                "change", ChangedContract);

        /// <summary>
        /// Gets the event raised when the widget items collection changes.
        /// </summary>
        public ReactiveEvent<TestWidgetSyncFusionItemsChangedArgs> ItemsChanged =>
            new ReactiveEvent<TestWidgetSyncFusionItemsChangedArgs>(
                "items-changed", ItemsChangedContract);
    }
}
