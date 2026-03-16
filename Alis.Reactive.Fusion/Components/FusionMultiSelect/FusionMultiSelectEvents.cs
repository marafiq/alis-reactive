namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionMultiSelect.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionMultiSelectEvents
    {
        public static readonly FusionMultiSelectEvents Instance = new FusionMultiSelectEvents();
        private FusionMultiSelectEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionMultiSelectChangeArgs> Changed =>
            new TypedEventDescriptor<FusionMultiSelectChangeArgs>(
                "change", new FusionMultiSelectChangeArgs());
    }
}
