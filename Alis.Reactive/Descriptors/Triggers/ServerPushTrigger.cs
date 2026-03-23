using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    /// <summary>
    /// Fires when the server sends a message via Server-Sent Events (SSE).
    /// Uses native browser EventSource API — zero JS library, auto-reconnect.
    ///
    /// Runtime: new EventSource(url) → addEventListener(eventType ?? "message")
    /// Payload: JSON.parse(event.data) → ExecContext.evt
    /// </summary>
    public sealed class ServerPushTrigger : Trigger
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "server-push";

        /// <summary>SSE endpoint URL (e.g. "/api/notifications/stream").</summary>
        public string Url { get; }

        /// <summary>
        /// Optional SSE event type filter. When set, listens for named events
        /// via addEventListener(eventType). When null, listens for unnamed messages
        /// via addEventListener("message"). Maps to the "event:" field in the SSE protocol.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? EventType { get; }

        public ServerPushTrigger(string url, string? eventType = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new System.ArgumentException("url is required", nameof(url));
            if (eventType != null && string.IsNullOrWhiteSpace(eventType))
                throw new System.ArgumentException("eventType cannot be empty when provided", nameof(eventType));
            Url = url;
            EventType = eventType;
        }
    }
}
