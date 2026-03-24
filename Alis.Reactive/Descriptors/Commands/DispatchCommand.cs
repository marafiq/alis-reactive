using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Commands
{
    public sealed class DispatchCommand : Command
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "dispatch";

        public string Event { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Payload { get; }

        public DispatchCommand(string @event, object? payload = null, Guard? when = null)
            : base(when)
        {
            Event = @event;
            Payload = payload;
        }

        internal override Command WithGuard(Guard guard)
        {
            return new DispatchCommand(Event, Payload, guard);
        }
    }
}
