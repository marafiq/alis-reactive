namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionMultiColumnComboBox"/> component.
    /// </summary>
    public sealed class FusionMultiColumnComboBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionMultiColumnComboBoxEvents Instance = new FusionMultiColumnComboBoxEvents();
        private FusionMultiColumnComboBoxEvents() { }

        /// <summary>Fires when the selected value changes.</summary>
        public TypedEvent<FusionMultiColumnComboBoxChangeArgs> Changed =>
            new TypedEvent<FusionMultiColumnComboBoxChangeArgs>(
                "change", new FusionMultiColumnComboBoxChangeArgs());
    }
}
