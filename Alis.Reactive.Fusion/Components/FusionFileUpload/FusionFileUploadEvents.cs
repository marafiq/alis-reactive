using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed reactive events for the <see cref="FusionFileUpload"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Selected, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionFileUploadEvents
    {
        private static readonly CapabilityProperty FilesCountEventMember = CapabilityProperty.Named("filesCount");
        private static readonly CapabilityProperty IsInteractedEventMember = CapabilityProperty.Named("isInteracted");

        private static readonly EventContractAuthoring SelectedContract =
            EventPayloadContractAuthoring.Define<FusionFileUploadSelectedArgs>(payload =>
            {
                payload.Read(args => args.FilesCount, FilesCountEventMember);
                payload.Read(args => args.IsInteracted, IsInteractedEventMember);
            });

        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionFileUploadEvents Instance = new FusionFileUploadEvents();
        private FusionFileUploadEvents() { }

        /// <summary>Fires when files are selected (SF "selected" event).</summary>
        public ReactiveEvent<FusionFileUploadSelectedArgs> Selected =>
            new ReactiveEvent<FusionFileUploadSelectedArgs>(
                "selected", SelectedContract);
    }
}
