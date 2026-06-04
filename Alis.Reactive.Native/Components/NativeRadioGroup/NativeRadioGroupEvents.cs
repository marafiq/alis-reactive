namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeRadioGroup"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these descriptors to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeRadioGroupEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeRadioGroupEvents Instance = new NativeRadioGroupEvents();
        private NativeRadioGroupEvents() { }

        /// <summary>
        /// Fires when the user selects a different radio option.
        /// </summary>
        public TypedEvent<NativeRadioGroupChangeArgs> Changed =>
            new TypedEvent<NativeRadioGroupChangeArgs>(
                "change", new NativeRadioGroupChangeArgs());
    }
}
