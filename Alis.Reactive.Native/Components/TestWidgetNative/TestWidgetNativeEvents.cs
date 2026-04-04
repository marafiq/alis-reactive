using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Exposes the reactive events available on <see cref="TestWidgetNative"/>.
    /// </summary>
    public sealed class TestWidgetNativeEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.FromSegments("value", NativeEventPaths.FromCurrentTarget(TestWidgetNative.Value.Path));

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<TestWidgetNativeChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
            });

        /// <summary>Gets the singleton event surface instance.</summary>
        public static readonly TestWidgetNativeEvents Instance = new TestWidgetNativeEvents();
        private TestWidgetNativeEvents() { }

        /// <summary>Gets the widget change event.</summary>
        public ReactiveEvent<TestWidgetNativeChangeArgs> Changed =>
            new ReactiveEvent<TestWidgetNativeChangeArgs>(
                "change", ChangedContract);
    }
}
