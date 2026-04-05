namespace Alis.Reactive
{
    /// <summary>
    /// Typed event reference for component .Reactive() extensions.
    /// Holds the JS event name and provides compile-time type safety for event args.
    /// </summary>
    public sealed class TypedEventDescriptor<TArgs>
    {
        public string JsEvent { get; }
        public TArgs Args { get; }

        internal TypedEventDescriptor(string jsEvent, TArgs args)
        {
            JsEvent = jsEvent;
            Args = args;
        }
    }
}
