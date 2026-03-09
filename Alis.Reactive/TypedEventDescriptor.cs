namespace Alis.Reactive
{
    /// <summary>
    /// Wraps a JS event name with a typed args instance.
    /// Enables C# generic inference in the .Reactive() extension:
    ///
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { args.Value... })
    ///
    /// The compiler infers TArgs from the eventSelector return type.
    /// TArgs is a descriptor marker (e.g., FusionNumericTextBoxChangeArgs).
    /// </summary>
    public sealed class TypedEventDescriptor<TArgs>
    {
        public string JsEvent { get; }
        public TArgs Args { get; }

        public TypedEventDescriptor(string jsEvent, TArgs args)
        {
            JsEvent = jsEvent;
            Args = args;
        }
    }
}
