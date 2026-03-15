using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Commands
{
    public sealed class DispatchCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "dispatch";

        public string Event { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Payload { get; }

        public DispatchCommand(string @event, object? payload = null)
        {
            Event = @event;
            Payload = payload;
        }
    }
}
