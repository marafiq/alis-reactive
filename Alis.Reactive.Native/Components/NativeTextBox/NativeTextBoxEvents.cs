namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeTextBox.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class NativeTextBoxEvents
    {
        public static readonly NativeTextBoxEvents Instance = new NativeTextBoxEvents();
        private NativeTextBoxEvents() { }

        /// <summary>Fires when the user changes the input value (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeTextBoxChangeArgs> Changed =>
            new TypedEventDescriptor<NativeTextBoxChangeArgs>(
                "change", new NativeTextBoxChangeArgs());
    }
}
