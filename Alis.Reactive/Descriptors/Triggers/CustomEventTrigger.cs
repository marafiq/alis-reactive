using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    public sealed class CustomEventTrigger : Trigger
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "custom-event";

        public string Event { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal CustomEventTrigger(string @event)
        {
            Event = @event;
        }
    }
}
