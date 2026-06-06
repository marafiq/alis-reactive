namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionMultiSelect"/> component.
    /// </summary>
    public sealed class FusionMultiSelectEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionMultiSelectEvents Instance = new FusionMultiSelectEvents();
        private FusionMultiSelectEvents() { }

        /// <summary>Fires when the selected value array changes.</summary>
        public TypedEvent<FusionMultiSelectChangeArgs> Changed =>
            new TypedEvent<FusionMultiSelectChangeArgs>(
                "change", new FusionMultiSelectChangeArgs());

        /// <summary>Fires when the user types to filter.</summary>
        public TypedEvent<FusionMultiSelectFilteringArgs> Filtering =>
            new TypedEvent<FusionMultiSelectFilteringArgs>(
                "filtering", new FusionMultiSelectFilteringArgs());
    }
}
