namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionFileUpload"/> component.
    /// </summary>
    public sealed class FusionFileUploadEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionFileUploadEvents Instance = new FusionFileUploadEvents();
        private FusionFileUploadEvents() { }

        /// <summary>Fires when files are selected.</summary>
        public TypedEvent<FusionFileUploadSelectedArgs> Selected =>
            new TypedEvent<FusionFileUploadSelectedArgs>(
                "selected", new FusionFileUploadSelectedArgs());
    }
}
