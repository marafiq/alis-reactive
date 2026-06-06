namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed events exposed by <see cref="NativeTextArea"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these events to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeTextAreaEvents
    {
        /// <summary>
        /// Selector instance for <c>.Reactive()</c> event lambdas.
        /// </summary>
        public static readonly NativeTextAreaEvents Instance = new NativeTextAreaEvents();
        private NativeTextAreaEvents() { }

        /// <summary>
        /// Fires when the user changes the textarea value and leaves the field.
        /// </summary>
        public TypedEvent<NativeTextAreaChangeArgs> Changed =>
            new TypedEvent<NativeTextAreaChangeArgs>(
                "change", new NativeTextAreaChangeArgs());
    }
}
