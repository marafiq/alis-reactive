namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Events available on NativeCheckList.
    /// Singleton instance — used with .Reactive() event selector lambda:
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { ... })
    /// </summary>
    public sealed class NativeCheckListEvents
    {
        public static readonly NativeCheckListEvents Instance = new NativeCheckListEvents();
        private NativeCheckListEvents() { }

        /// <summary>Fires when the user checks or unchecks a checkbox option (DOM "change" event).</summary>
        public TypedEventDescriptor<NativeCheckListChangeArgs> Changed =>
            new TypedEventDescriptor<NativeCheckListChangeArgs>(
                "change", new NativeCheckListChangeArgs());
    }
}
