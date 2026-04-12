namespace Alis.Reactive
{
    /// <summary>
    /// Typed event reference for component .Reactive() extensions.
    /// Holds the JS event name and provides compile-time type safety for event args.
    /// </summary>
    public sealed class TypedEvent<TArgs>
    {
        /// <summary>Gets the JavaScript event name.</summary>
        public string JsEvent { get; }
        /// <summary>Gets the typed event arguments instance.</summary>
        public TArgs Args { get; }

        internal TypedEvent(string jsEvent, TArgs args)
        {
            JsEvent = jsEvent;
            Args = args;
        }
    }
}
