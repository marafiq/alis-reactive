namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Typed events exposed by <see cref="NativeDropDown"/>.
    /// </summary>
    /// <remarks>
    /// <c>.Reactive()</c> uses these events to select the DOM event name
    /// and the payload type passed to the pipeline lambda.
    /// </remarks>
    public sealed class NativeDropDownEvents
    {
        /// <summary>
        /// Selector instance for <c>.Reactive()</c> event lambdas.
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
