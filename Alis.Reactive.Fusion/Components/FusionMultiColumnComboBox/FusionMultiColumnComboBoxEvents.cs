namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed event descriptors for the <see cref="FusionMultiColumnComboBox"/> component.
    /// </summary>
    /// <remarks>
    /// Select an event via the <c>.Reactive()</c> lambda:
    /// <c>.Reactive(plan, evt =&gt; evt.Changed, (args, p) =&gt; { ... })</c>.
    /// </remarks>
    public sealed class FusionMultiColumnComboBoxEvents
    {
        /// <summary>Shared instance used by the <c>.Reactive()</c> event selector.</summary>
        public static readonly FusionMultiColumnComboBoxEvents Instance = new FusionMultiColumnComboBoxEvents();
        private FusionMultiColumnComboBoxEvents() { }

        /// <summary>Fires when the selected value changes (SF "change" event).</summary>
        public TypedEvent<FusionMultiColumnComboBoxChangeArgs> Changed =>
            new TypedEvent<FusionMultiColumnComboBoxChangeArgs>(
                "change", new FusionMultiColumnComboBoxChangeArgs());
    }
}
