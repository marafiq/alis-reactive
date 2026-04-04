using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal delegate void EventContractAuthoring(
        PlanContractCatalog contracts,
        string objectContractKey,
        string eventName,
        EventContract eventContract);

    /// <summary>
    /// Carries the browser event name and typed payload selected by a component's event surface.
    /// Enables C# generic inference in the <c>.Reactive()</c> extension:
    ///
    ///   .Reactive(plan, evt => evt.Changed, (args, p) => { args.Value... })
    ///
    /// The compiler infers <typeparamref name="TPayload"/> from the event selector return type.
    /// <typeparamref name="TPayload"/> is used only for compile-time access to event payload members.
    /// No payload instance is created by the framework.
    /// </summary>
    public sealed class ReactiveEvent<TPayload>
    {
        /// <summary>
        /// Gets the browser event name emitted by the component contract.
        /// </summary>
        public string EventName { get; }

        internal EventContractAuthoring? ContractAuthoring { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on framework-owned contract types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ReactiveEvent(
            string eventName,
            EventContractAuthoring? contractAuthoring = null)
        {
            EventName = eventName;
            ContractAuthoring = contractAuthoring;
        }
    }
}
