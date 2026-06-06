namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed events exposed by <see cref="NativeTextBox"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these events to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeTextBoxEvents
    {
        /// <summary>
        /// Selector instance for <c>.Reactive()</c> event lambdas.
        /// </summary>
        public static readonly NativeTextBoxEvents Instance = new NativeTextBoxEvents();
        private NativeTextBoxEvents() { }

        /// <summary>
        /// Fires when the user changes the input value and leaves the field.
        /// </summary>
        public TypedEvent<NativeTextBoxChangeArgs> Changed =>
            new TypedEvent<NativeTextBoxChangeArgs>(
                "change", new NativeTextBoxChangeArgs());
    }
}
