namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeHiddenField.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// Hidden inputs rarely fire change events — this exists for completeness.
    /// </summary>
    public sealed class NativeHiddenFieldEvents
    {
        public static readonly NativeHiddenFieldEvents Instance = new NativeHiddenFieldEvents();
        private NativeHiddenFieldEvents() { }

        /// <summary>Fires when the hidden input value changes (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeHiddenFieldChangeArgs> Changed =>
            new TypedEventDescriptor<NativeHiddenFieldChangeArgs>(
                "change", new NativeHiddenFieldChangeArgs());
    }
}
