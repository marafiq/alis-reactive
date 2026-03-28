using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Triggers
{
    /// <summary>
    /// Fires when a SignalR Hub method is invoked by the server.
    /// Uses @microsoft/signalr client — HubConnection.on(methodName).
    ///
    /// Runtime: HubConnectionBuilder → build() → start() → on(methodName)
    /// Payload: Hub method arguments → ExecContext.evt
    /// Connection: singleton per hubUrl, shared across all triggers on same hub.
    /// </summary>
    public sealed class SignalRTrigger : Trigger
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "signalr";

        /// <summary>Hub URL (e.g. "/hubs/notifications"). Connection is shared
        /// across all triggers with the same hubUrl.</summary>
        public string HubUrl { get; }

        /// <summary>Server method name to listen for (e.g. "ReceiveNotification").
        /// Maps to connection.on(methodName, handler).</summary>
        public string MethodName { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal SignalRTrigger(string hubUrl, string methodName)
        {
            if (string.IsNullOrWhiteSpace(hubUrl))
                throw new System.ArgumentException("hubUrl is required", nameof(hubUrl));
            if (string.IsNullOrWhiteSpace(methodName))
                throw new System.ArgumentException("methodName is required", nameof(methodName));
            HubUrl = hubUrl;
            MethodName = methodName;
        }
    }
}
