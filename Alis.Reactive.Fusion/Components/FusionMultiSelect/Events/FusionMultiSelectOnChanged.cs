namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionMultiSelect.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("peanuts")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class FusionMultiSelectChangeArgs
    {
        /// <summary>The selected value(s).</summary>
        public string? Value { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionMultiSelectChangeArgs() { }
    }
}
