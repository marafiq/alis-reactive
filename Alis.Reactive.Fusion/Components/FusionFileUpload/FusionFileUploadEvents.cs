namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionFileUpload.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Selected, (args, p) => { ... })
    /// </summary>
    public sealed class FusionFileUploadEvents
    {
        public static readonly FusionFileUploadEvents Instance = new FusionFileUploadEvents();
        private FusionFileUploadEvents() { }

        /// <summary>Fires when files are selected (SF "selected" event).</summary>
        public TypedEventDescriptor<FusionFileUploadSelectedArgs> Selected =>
            new TypedEventDescriptor<FusionFileUploadSelectedArgs>(
                "selected", new FusionFileUploadSelectedArgs());
    }
}
