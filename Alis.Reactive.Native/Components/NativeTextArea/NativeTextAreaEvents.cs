namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeTextArea.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class NativeTextAreaEvents
    {
        public static readonly NativeTextAreaEvents Instance = new NativeTextAreaEvents();
        private NativeTextAreaEvents() { }

        /// <summary>Fires when the user changes the textarea value (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeTextAreaChangeArgs> Changed =>
            new TypedEventDescriptor<NativeTextAreaChangeArgs>(
                "change", new NativeTextAreaChangeArgs());
    }
}
