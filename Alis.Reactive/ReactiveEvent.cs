namespace Alis.Reactive
{
    /// <summary>
    /// Carries the browser event name and typed payload prototype selected by a component's
    /// events surface.
    /// Enables C# generic inference in the <c>.Reactive()</c> extension:
    ///
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { args.Value... })
    ///
    /// The compiler infers <typeparamref name="TPayload"/> from the event selector return type.
    /// <typeparamref name="TPayload"/> is used only for compile-time access to event payload members.
    /// </summary>
    public sealed class ReactiveEvent<TPayload>
    {
        /// <summary>
        /// Gets the browser event name emitted by the component contract.
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Gets the typed payload surface used for compile-time event access in the DSL.
        /// </summary>
        public TPayload Payload { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on framework-owned contract types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ReactiveEvent(string eventName, TPayload payload)
        {
            EventName = eventName;
            Payload = payload;
        }
    }
}
