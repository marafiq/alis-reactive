using Alis.Reactive;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeDropDown.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class NativeDropDownEvents
    {
        public static readonly NativeDropDownEvents Instance = new NativeDropDownEvents();
        private NativeDropDownEvents() { }

        /// <summary>Fires when the user selects a different option (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeDropDownChangeArgs> Changed =>
            new TypedEventDescriptor<NativeDropDownChangeArgs>(
                "change", new NativeDropDownChangeArgs());
    }
}
