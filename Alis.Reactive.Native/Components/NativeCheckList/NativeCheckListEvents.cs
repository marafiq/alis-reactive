namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeCheckList"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these descriptors to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeCheckListEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeCheckListEvents Instance = new NativeCheckListEvents();
        private NativeCheckListEvents() { }

        /// <summary>
        /// Fires when the user checks or unchecks any checkbox in the list.
        /// </summary>
        public TypedEvent<NativeCheckListChangeArgs> Changed =>
            new TypedEvent<NativeCheckListChangeArgs>(
                "change", new NativeCheckListChangeArgs());
    }
}
