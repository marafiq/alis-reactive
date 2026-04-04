using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed reactive events for <see cref="NativeCheckBox"/>.
    /// </summary>
    /// <remarks>
    /// Used with the <c>.Reactive()</c> event selector lambda:
    /// <code>.Reactive(plan, evt => evt.Changed, (args, p) => { ... })</code>
    /// </remarks>
    public sealed class NativeCheckBoxEvents
    {
        private static readonly CapabilityProperty CheckedEventMember = CapabilityProperty.FromSegments("checked", NativeEventPaths.FromCurrentTarget(NativeCheckBox.Checked.Path));

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<NativeCheckBoxChangeArgs>(payload =>
            {
                payload.Read(args => args.Checked, CheckedEventMember);
            });

        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeCheckBoxEvents Instance = new NativeCheckBoxEvents();
        private NativeCheckBoxEvents() { }

        /// <summary>
        /// Fires when the user checks or unchecks the checkbox.
        /// </summary>
        public ReactiveEvent<NativeCheckBoxChangeArgs> Changed =>
            new ReactiveEvent<NativeCheckBoxChangeArgs>(
                "change", ChangedContract);
    }
}
