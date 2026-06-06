namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed events exposed by the <see cref="FusionMultiColumnComboBox"/> component.
    /// </summary>
    public sealed class FusionMultiColumnComboBoxEvents
    {
        /// <summary>Selector instance for <c>.Reactive()</c> event lambdas.</summary>
        public static readonly FusionMultiColumnComboBoxEvents Instance = new FusionMultiColumnComboBoxEvents();
        private FusionMultiColumnComboBoxEvents() { }

        /// <summary>Fires when selected value changes.</summary>
        public TypedEvent<FusionMultiColumnComboBoxChangeArgs> Changed =>
            new TypedEvent<FusionMultiColumnComboBoxChangeArgs>(
                "change", new FusionMultiColumnComboBoxChangeArgs());
    }
}
