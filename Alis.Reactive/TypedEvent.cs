namespace Alis.Reactive
{
    /// <summary>
    /// Typed event reference for component .Reactive() extensions.
    /// Holds the JS event name and provides compile-time type safety for event args.
    /// </summary>
    public sealed class TypedEvent<TArgs>
    {
        public string JsEvent { get; }
        public TArgs Args { get; }

        internal TypedEvent(string jsEvent, TArgs args)
        {
            JsEvent = jsEvent;
            Args = args;
        }
    }
}
