namespace Alis.Reactive
{
    /// <summary>
    /// Represents a component event selected by a component <c>.Reactive(...)</c> overload.
    /// </summary>
    /// <remarks>
    /// Component event collections return this value so <c>.Reactive(...)</c> can write the
    /// event name into the Reactive Plan and expose a typed payload placeholder to the
    /// authoring lambda.
    /// </remarks>
    /// <typeparam name="TArgs">The event payload type available to the reaction pipeline.</typeparam>
    public sealed class TypedEvent<TArgs>
    {
        /// <summary>Event name emitted into the Reactive Plan.</summary>
        public string ObjectEvent { get; }

        /// <summary>Placeholder used to author reads from the event payload.</summary>
        public TArgs Args { get; }

        internal TypedEvent(string jsEvent, TArgs args)
        {
            ObjectEvent = jsEvent;
            Args = args;
        }
    }
}
