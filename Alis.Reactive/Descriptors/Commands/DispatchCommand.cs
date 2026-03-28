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

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal DispatchCommand(string @event, object? payload = null, Guard? when = null)
            : base(when)
        {
            Event = @event;
            Payload = payload;
        }

        protected override Command CloneWithGuard(Guard guard)
        {
            return new DispatchCommand(Event, Payload, guard);
        }
    }
}
