using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed reactive events for <see cref="NativeCheckList"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeCheckListEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.FromSegments("value", NativeEventPaths.FromCurrentTarget(NativeCheckList.Value.Path));

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<NativeCheckListChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
            });

        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeCheckListEvents Instance = new NativeCheckListEvents();
        private NativeCheckListEvents() { }

        /// <summary>
        /// Fires when the user checks or unchecks any checkbox in the list.
        /// </summary>
        public ReactiveEvent<NativeCheckListChangeArgs> Changed =>
            new ReactiveEvent<NativeCheckListChangeArgs>(
                "change", ChangedContract);
    }
}
