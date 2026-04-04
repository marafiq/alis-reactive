using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed reactive events for <see cref="NativeRadioGroup"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeRadioGroupEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.FromSegments("value", NativeEventPaths.FromCurrentTarget(NativeRadioGroup.Value.Path));

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<NativeRadioGroupChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
            });

        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeRadioGroupEvents Instance = new NativeRadioGroupEvents();
        private NativeRadioGroupEvents() { }

        /// <summary>
        /// Fires when the user selects a different radio option.
        /// </summary>
        public ReactiveEvent<NativeRadioGroupChangeArgs> Changed =>
            new ReactiveEvent<NativeRadioGroupChangeArgs>(
                "change", ChangedContract);
    }
}
