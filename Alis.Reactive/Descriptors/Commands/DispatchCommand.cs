using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    /// <summary>
    /// Dispatches a custom DOM event on <c>document</c>.
    /// </summary>
    public sealed class DispatchCommand : Command
    {
        /// <summary>Gets the discriminator written to plan JSON.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "dispatch";

        /// <summary>Gets the event name dispatched on <c>document</c>.</summary>
        public string Event { get; }

        /// <summary>
        /// Gets the optional object-shaped payload resolved into the final
        /// <c>CustomEvent.detail</c> object at dispatch time.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public DispatchPayload? Payload { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal DispatchCommand(string @event, DispatchPayload? payload = null)
        {
            Event = @event;
            Payload = payload;
        }
    }
}
