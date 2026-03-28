using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    /// <summary>
    /// Gathers a value from the event payload (execution context) at runtime.
    /// The path is a dot-notation walk into ctx.evt (e.g., "evt.text" → ctx.evt.text).
    /// Used when a trigger's event args carry the value needed for the HTTP request,
    /// such as the typed text from a filtering event.
    /// </summary>
    public sealed class EventGather : GatherItem
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "event";

        /// <summary>Query parameter name (e.g., "MedicationType").</summary>
        public string Param { get; }

        /// <summary>Dot-notation path into event payload (e.g., "evt.text").</summary>
        public string Path { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal EventGather(string param, string path)
        {
            Param = param;
            Path = path;
        }
    }
}
