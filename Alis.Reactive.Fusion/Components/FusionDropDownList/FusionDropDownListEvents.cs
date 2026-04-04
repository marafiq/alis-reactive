using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionDropDownList"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionDropDownListEvents
    {
        private static readonly CapabilityProperty ValueEventMember = CapabilityProperty.Named("value");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring ChangedContract =
            EventPayloadContractAuthoring.Define<FusionDropDownListChangeArgs>(payload =>
            {
                payload.Read(args => args.Value, ValueEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        private static readonly EventContractAuthoring FocusContract =
            EventPayloadContractAuthoring.Define<FusionDropDownListFocusArgs>(_ => { });
        private static readonly EventContractAuthoring BlurContract =
            EventPayloadContractAuthoring.Define<FusionDropDownListBlurArgs>(_ => { });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionDropDownListEvents Instance = new FusionDropDownListEvents();
        private FusionDropDownListEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public ReactiveEvent<FusionDropDownListChangeArgs> Changed =>
            new ReactiveEvent<FusionDropDownListChangeArgs>(
                "change", ChangedContract);

        /// <summary>Fires when the component receives focus (SF "focus" event).</summary>
        public ReactiveEvent<FusionDropDownListFocusArgs> Focus =>
            new ReactiveEvent<FusionDropDownListFocusArgs>(
                "focus", FocusContract);

        /// <summary>Fires when the component loses focus (SF "blur" event).</summary>
        public ReactiveEvent<FusionDropDownListBlurArgs> Blur =>
            new ReactiveEvent<FusionDropDownListBlurArgs>(
                "blur", BlurContract);
    }
}
