using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed reactive events for <see cref="NativeDropDown"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeDropDownEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.FromSegments("value", NativeEventPaths.FromCurrentTarget(NativeDropDown.Value.Path));

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<NativeDropDownChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
            });

        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeDropDownEvents Instance = new NativeDropDownEvents();
        private NativeDropDownEvents() { }

        /// <summary>
        /// Fires when the user selects a different option.
        /// </summary>
        public ReactiveEvent<NativeDropDownChangeArgs> Changed =>
            new ReactiveEvent<NativeDropDownChangeArgs>(
                "change", ChangedContract);
    }
}
