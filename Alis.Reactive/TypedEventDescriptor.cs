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

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal TypedEventDescriptor(string jsEvent, TArgs args)
        {
            JsEvent = jsEvent;
            Args = args;
        }
    }
}
