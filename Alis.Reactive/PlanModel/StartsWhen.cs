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
        internal static StartsWhen DocumentEvent(string eventName) =>
            new DocumentEventTrigger(eventName, PayloadContract.Untyped);

        internal static StartsWhen DocumentEvent(string eventName, PayloadContract payloadType) =>
            new DocumentEventTrigger(eventName, payloadType);

        internal static StartsWhen ComponentEvent(string component, string eventName) => new ComponentEventTrigger(component, eventName);

        internal static StartsWhen ServerPush(string url) =>
            new ServerPushTrigger(url, ServerPushEventFilter.AnyEvent());

        internal static StartsWhen ServerPush(string url, string eventName) =>
            new ServerPushTrigger(url, ServerPushEventFilter.NamedEvent(eventName));

        internal static StartsWhen ServerPush(string url, string eventName, PayloadContract payloadType) =>
            new ServerPushTrigger(url, ServerPushEventFilter.NamedEvent(eventName, payloadType));

        internal static StartsWhen SignalR(string hubUrl, string method) =>
            new SignalRTrigger(hubUrl, method, PayloadContract.Untyped);

        internal static StartsWhen SignalR(string hubUrl, string method, PayloadContract payloadType) =>
            new SignalRTrigger(hubUrl, method, payloadType);
    }

    internal sealed class PageReadyTrigger : StartsWhen
    {
        public string Kind => "page-ready";
    }

    internal sealed class DocumentEventTrigger : StartsWhen
    {
        private readonly EventName _event;
        private readonly PayloadContract _payloadType;

        public string Kind => "document-event";
        public string Event => _event.Value;
        public PayloadContract PayloadType => _payloadType;

        internal DocumentEventTrigger(string eventName, PayloadContract payloadType)
        {
            _event = EventName.Of(eventName);
            _payloadType = payloadType ?? throw new ArgumentNullException(nameof(payloadType));
        }
    }

    internal sealed class ComponentEventTrigger : StartsWhen
    {
        private readonly ComponentKey _component;
        private readonly EventName _event;

        public string Kind => "component-event";
        public string Component => _component.Value;
        public string Event => _event.Value;
        internal ComponentKey ComponentKey => _component;
        internal EventName EventName => _event;

        internal ComponentEventTrigger(string component, string eventName)
        {
            _component = ComponentKey.Of(component);
            _event = EventName.Of(eventName);
        }
    }

    internal sealed class ServerPushTrigger : StartsWhen
    {
        private readonly RequestUrl _url;
        private readonly ServerPushEventFilter _filter;

        public string Kind => "server-push";
        public string Url => _url.Value;
        public ServerPushEventFilter EventFilter => _filter;

        internal ServerPushTrigger(string url, ServerPushEventFilter filter)
        {
            _url = RequestUrl.Of(url);
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }
    }

    internal sealed class SignalRTrigger : StartsWhen
    {
        private readonly RequestUrl _hubUrl;
        private readonly MemberName _method;
        private readonly PayloadContract _payloadType;

        public string Kind => "signalr";
        public string HubUrl => _hubUrl.Value;
        public string Method => _method.Value;
        public PayloadContract PayloadType => _payloadType;

        internal SignalRTrigger(string hubUrl, string method, PayloadContract payloadType)
        {
            _hubUrl = RequestUrl.Of(hubUrl);
            _method = MemberName.Of(method);
            _payloadType = payloadType ?? throw new ArgumentNullException(nameof(payloadType));
        }
    }

    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ServerPushEventFilter>))]
    public abstract class ServerPushEventFilter
    {
        private protected ServerPushEventFilter(PayloadContract payloadType)
        {
            _payloadType = payloadType ?? throw new ArgumentNullException(nameof(payloadType));
        }

        private readonly PayloadContract _payloadType;

        public abstract string Kind { get; }
        public PayloadContract PayloadType => _payloadType;

        internal static ServerPushEventFilter AnyEvent() =>
            new AnyServerPushEvent(PayloadContract.Untyped);

        internal static ServerPushEventFilter NamedEvent(string eventName) =>
            new NamedServerPushEvent(eventName, PayloadContract.Untyped);

        internal static ServerPushEventFilter NamedEvent(string eventName, PayloadContract payloadType) =>
            new NamedServerPushEvent(eventName, payloadType);
    }

    internal sealed class AnyServerPushEvent : ServerPushEventFilter
    {
        internal AnyServerPushEvent(PayloadContract payloadType) : base(payloadType) { }

        public override string Kind => "any";
    }

    internal sealed class NamedServerPushEvent : ServerPushEventFilter
    {
        private readonly EventName _event;

        internal NamedServerPushEvent(string eventName, PayloadContract payloadType) : base(payloadType)
        {
            _event = EventName.Of(eventName);
        }

        public override string Kind => "named";
        public string Event => _event.Value;
    }
}
