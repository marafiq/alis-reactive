using Alis.Reactive;

namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeCheckBox.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class NativeCheckBoxEvents
    {
        public static readonly NativeCheckBoxEvents Instance = new NativeCheckBoxEvents();
        private NativeCheckBoxEvents() { }

        /// <summary>Fires when the user toggles the checkbox (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeCheckBoxChangeArgs> Changed =>
            new TypedEventDescriptor<NativeCheckBoxChangeArgs>(
                "change", new NativeCheckBoxChangeArgs());
    }
}
