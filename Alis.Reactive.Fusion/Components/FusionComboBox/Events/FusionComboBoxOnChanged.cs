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
        /// <summary>Selected value.</summary>
        public string? Value { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
