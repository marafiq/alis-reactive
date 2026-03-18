namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeRadioGroup.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class NativeRadioGroupEvents
    {
        public static readonly NativeRadioGroupEvents Instance = new NativeRadioGroupEvents();
        private NativeRadioGroupEvents() { }

        /// <summary>Fires when the user selects a different radio option (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeRadioGroupChangeArgs> Changed =>
            new TypedEventDescriptor<NativeRadioGroupChangeArgs>(
                "change", new NativeRadioGroupChangeArgs());
    }
}
