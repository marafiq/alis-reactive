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

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal DispatchCommand(string @event, object? payload = null)
        {
            Event = @event;
            Payload = payload;
        }
    }
}
