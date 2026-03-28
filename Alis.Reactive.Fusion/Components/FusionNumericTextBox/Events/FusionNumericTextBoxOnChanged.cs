namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionNumericTextBox"/> value changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Value).Gte(100m)</c>.
    /// </remarks>
    public class FusionNumericTextBoxChangeArgs
    {
        /// <summary>Gets or sets the new numeric value.</summary>
        public decimal Value { get; set; }

        /// <summary>Gets or sets the previous numeric value.</summary>
        public decimal PreviousValue { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionNumericTextBoxChangeArgs() { }
    }
}
