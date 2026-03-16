namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Events available on FusionMultiColumnComboBox.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class FusionMultiColumnComboBoxEvents
    {
        public static readonly FusionMultiColumnComboBoxEvents Instance = new FusionMultiColumnComboBoxEvents();
        private FusionMultiColumnComboBoxEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEventDescriptor<FusionMultiColumnComboBoxChangeArgs> Changed =>
            new TypedEventDescriptor<FusionMultiColumnComboBoxChangeArgs>(
                "change", new FusionMultiColumnComboBoxChangeArgs());
    }
}
