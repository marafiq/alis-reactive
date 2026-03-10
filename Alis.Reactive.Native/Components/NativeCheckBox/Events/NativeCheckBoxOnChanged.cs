namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeCheckBox.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Checked).Truthy()
    /// ExpressionPathHelper resolves x => x.Checked to "evt.checked".
    /// </summary>
    public class NativeCheckBoxChangeArgs
    {
        /// <summary>The checkbox's current checked state after the change.</summary>
        public bool? Checked { get; set; }

        public NativeCheckBoxChangeArgs() { }
    }
}
