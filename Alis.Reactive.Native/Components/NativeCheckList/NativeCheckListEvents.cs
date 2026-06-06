namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed events exposed by <see cref="NativeCheckList"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these events to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeCheckListEvents
    {
        /// <summary>
        /// Selector instance for <c>.Reactive()</c> event lambdas.
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
