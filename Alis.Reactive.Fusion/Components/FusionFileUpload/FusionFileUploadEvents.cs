namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionFileUpload"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Selected, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionFileUploadEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionFileUploadEvents Instance = new FusionFileUploadEvents();
        private FusionFileUploadEvents() { }

        /// <summary>Fires when files are selected (SF "selected" event).</summary>
        public TypedEvent<FusionFileUploadSelectedArgs> Selected =>
            new TypedEvent<FusionFileUploadSelectedArgs>(
                "selected", new FusionFileUploadSelectedArgs());
    }
}
