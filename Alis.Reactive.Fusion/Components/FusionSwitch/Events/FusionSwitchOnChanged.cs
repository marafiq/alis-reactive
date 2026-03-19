namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for FusionSwitch.Changed (SF "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Checked).Truthy()
    /// ExpressionPathHelper resolves x => x.Checked to "evt.checked".
    /// </summary>
    public class FusionSwitchChangeArgs
    {
        /// <summary>The switch's current checked state after the change.</summary>
        public bool Checked { get; set; }

        /// <summary>True if the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        public FusionSwitchChangeArgs() { }
    }
}
