namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed event descriptors for <see cref="NativeDropDown"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these descriptors to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeDropDownEvents
    {
        /// <summary>
        /// Shared instance used by the <c>.Reactive()</c> extension.
        /// </summary>
        public static readonly NativeDropDownEvents Instance = new NativeDropDownEvents();
        private NativeDropDownEvents() { }

        /// <summary>
        /// Fires when the user selects a different option.
        /// </summary>
        public TypedEvent<NativeDropDownChangeArgs> Changed =>
            new TypedEvent<NativeDropDownChangeArgs>(
                "change", new NativeDropDownChangeArgs());
    }
}
