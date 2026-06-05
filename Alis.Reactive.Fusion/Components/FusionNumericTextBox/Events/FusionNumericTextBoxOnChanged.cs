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
        /// <summary>New numeric value.</summary>
        public decimal Value { get; set; }

        /// <summary>Previous numeric value.</summary>
        public decimal PreviousValue { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
