namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionNumericTextBox.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Gte(100)
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class FusionNumericTextBoxChangeArgs
    {
        /// <summary>New numeric value.</summary>
        public decimal Value { get; set; }

        /// <summary>Previous numeric value.</summary>
        public decimal PreviousValue { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionNumericTextBoxChangeArgs() { }
    }
}
