using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    public sealed class CustomEventTrigger : Trigger
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "custom-event";

        public string Event { get; }

        public CustomEventTrigger(string @event)
        {
            Event = @event;
        }
    }
}
