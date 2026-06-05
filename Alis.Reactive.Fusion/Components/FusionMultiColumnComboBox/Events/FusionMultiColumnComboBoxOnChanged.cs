namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionMultiColumnComboBox"/> selection changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).Eq("1")</c>.
    /// </remarks>
    public class FusionMultiColumnComboBoxChangeArgs
    {
        /// <summary>Selected value.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
