using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Exposes the reactive events available on <see cref="NativeHiddenField"/>.
    /// </summary>
    public sealed class NativeHiddenFieldEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.FromSegments("value", NativeEventPaths.FromCurrentTarget(NativeHiddenField.Value.Path));

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<NativeHiddenFieldChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
            });

        /// <summary>Gets the singleton event surface instance.</summary>
        public static readonly NativeHiddenFieldEvents Instance = new NativeHiddenFieldEvents();
        private NativeHiddenFieldEvents() { }

        /// <summary>Gets the hidden-field change event.</summary>
        public ReactiveEvent<NativeHiddenFieldChangeArgs> Changed =>
            new ReactiveEvent<NativeHiddenFieldChangeArgs>(
                "change", ChangedContract);
    }
}
