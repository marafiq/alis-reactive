namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Payload for NativeCheckList.Changed (DOM "change" event).
    /// Properties are typed markers for expression-based condition sources:
    ///   p.When(args, x => x.Value).NotEmpty()
    /// ExpressionPathHelper resolves x => x.Value to "evt.value".
    /// Value holds the array of checked values (e.g. ["Peanuts","Dairy"]).
    /// </summary>
    public class NativeCheckListChangeArgs
    {
        /// <summary>Array of checked values after the change.</summary>
        public string[]? Value { get; set; }

        public NativeCheckListChangeArgs() { }
    }
}
