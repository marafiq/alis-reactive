namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeDropDown.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).Eq("active")
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// </summary>
    public class NativeDropDownChangeArgs
    {
        /// <summary>The selected option's value after the change.</summary>
        public string? Value { get; set; }

        public NativeDropDownChangeArgs() { }
    }
}
