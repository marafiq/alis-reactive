using System;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<StartsWhen>))]
    internal abstract class StartsWhen
    {
        private protected StartsWhen() { }

        internal static StartsWhen PageReady() => new PageReadyTrigger();
        internal static StartsWhen DocumentEvent(string eventName, string? payloadType = null) => new DocumentEventTrigger(eventName, payloadType);
        internal static StartsWhen ComponentEvent(string component, string eventName) => new ComponentEventTrigger(component, eventName);
        internal static StartsWhen ServerPush(string url, string? eventName = null, string? payloadType = null) => new ServerPushTrigger(url, eventName, payloadType);
        internal static StartsWhen SignalR(string hubUrl, string method, string? payloadType = null) => new SignalRTrigger(hubUrl, method, payloadType);
    }

    internal sealed class PageReadyTrigger : StartsWhen
    {
        public string Kind => "page-ready";
    }

    internal sealed class DocumentEventTrigger : StartsWhen
    {
        public string Kind => "document-event";
        public string Event { get; }
        public string? PayloadType { get; }

        internal DocumentEventTrigger(string eventName, string? payloadType)
        {
            Event = eventName ?? throw new ArgumentNullException(nameof(eventName));
            PayloadType = payloadType;
        }
    }

    internal sealed class ComponentEventTrigger : StartsWhen
    {
        public string Kind => "component-event";
        public string Component { get; }
        public string Event { get; }

        internal ComponentEventTrigger(string component, string eventName)
        {
            Component = component ?? throw new ArgumentNullException(nameof(component));
            Event = eventName ?? throw new ArgumentNullException(nameof(eventName));
        }
    }

    internal sealed class ServerPushTrigger : StartsWhen
    {
        public string Kind => "server-push";
        public string Url { get; }
        public string? Event { get; }
        public string? PayloadType { get; }

        internal ServerPushTrigger(string url, string? eventName, string? payloadType)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            Event = eventName;
            PayloadType = payloadType;
        }
    }

    internal sealed class SignalRTrigger : StartsWhen
    {
        public string Kind => "signalr";
        public string HubUrl { get; }
        public string Method { get; }
        public string? PayloadType { get; }

        internal SignalRTrigger(string hubUrl, string method, string? payloadType)
        {
            HubUrl = hubUrl ?? throw new ArgumentNullException(nameof(hubUrl));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            PayloadType = payloadType;
        }
    }
}
