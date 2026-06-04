namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionComboBox"/> selection changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).Eq("alice")</c>.
    /// </remarks>
    public class FusionComboBoxChangeArgs
    {
        /// <summary>Gets or sets the selected value.</summary>
        public string? Value { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }
    }
}
