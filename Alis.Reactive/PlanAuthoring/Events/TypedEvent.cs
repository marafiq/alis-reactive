namespace Alis.Reactive
{
    /// <summary>
    /// Describes a component event selected by a component <c>.Reactive(...)</c> overload.
    /// </summary>
    /// <remarks>
    /// Component event collections return this descriptor so the framework can write the
    /// event name into the Reactive Plan while giving the authoring lambda a typed payload
    /// placeholder for expression paths.
    /// </remarks>
    /// <typeparam name="TArgs">The event payload contract exposed to the reaction pipeline.</typeparam>
    public sealed class TypedEvent<TArgs>
    {
        /// <summary>Component-object event name written into the Reactive Plan.</summary>
        public string ObjectEvent { get; }

        /// <summary>Typed payload placeholder used to author event-payload reads.</summary>
        public TArgs Args { get; }

        internal TypedEvent(string jsEvent, TArgs args)
        {
            ObjectEvent = jsEvent;
            Args = args;
        }
    }
}
