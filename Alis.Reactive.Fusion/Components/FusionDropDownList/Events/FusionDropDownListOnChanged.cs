namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionDropDownList.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("US")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class FusionDropDownListChangeArgs
    {
        /// <summary>The selected value.</summary>
        public string? Value { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionDropDownListChangeArgs() { }
    }
}
