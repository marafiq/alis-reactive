namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed events exposed by <see cref="NativeCheckBox"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these events to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeCheckBoxEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeCheckBoxEvents Instance = new NativeCheckBoxEvents();
        private NativeCheckBoxEvents() { }

        /// <summary>
        /// Fires when the user checks or unchecks the checkbox.
        /// </summary>
        public TypedEvent<NativeCheckBoxChangeArgs> Changed =>
            new TypedEvent<NativeCheckBoxChangeArgs>(
                "change", new NativeCheckBoxChangeArgs());
    }
}
