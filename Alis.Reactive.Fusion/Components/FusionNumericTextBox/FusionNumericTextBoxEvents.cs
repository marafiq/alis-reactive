using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionNumericTextBox"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionNumericTextBoxEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.Named("value");
        private static readonly CapabilityProperty PreviousValueEventMember = CapabilityProperty.Named("previousValue");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionNumericTextBoxChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
                payload.Read(args => args.PreviousValue, PreviousValueEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        private static readonly EventContractAuthoring FocusContract =
            EventPayloadContractAuthoring.Define<FusionNumericTextBoxFocusArgs>(_ => { });
        private static readonly EventContractAuthoring BlurContract =
            EventPayloadContractAuthoring.Define<FusionNumericTextBoxBlurArgs>(_ => { });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionNumericTextBoxEvents Instance = new FusionNumericTextBoxEvents();
        private FusionNumericTextBoxEvents() { }

        /// <summary>Fires when the numeric value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionNumericTextBoxChangeArgs> Changed =>
            new ReactiveEvent<FusionNumericTextBoxChangeArgs>(
                "change", ChangedContract);

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public ReactiveEvent<FusionNumericTextBoxFocusArgs> Focus =>
            new ReactiveEvent<FusionNumericTextBoxFocusArgs>(
                "focus", FocusContract);

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public ReactiveEvent<FusionNumericTextBoxBlurArgs> Blur =>
            new ReactiveEvent<FusionNumericTextBoxBlurArgs>(
                "blur", BlurContract);
    }
}
