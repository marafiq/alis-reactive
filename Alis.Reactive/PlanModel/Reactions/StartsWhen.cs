using System;

namespace Alis.Reactive.PlanModel
{
    internal abstract class StartsWhen
    {
        private protected StartsWhen() { }

        internal static StartsWhen PageReady() => new PageReadyTrigger();

        internal static StartsWhen DocumentEvent(string eventName) =>
            new DocumentEventTrigger(eventName);

        internal static StartsWhen ComponentEvent(string component, string eventName) => new ComponentEventTrigger(component, eventName);

        internal static StartsWhen ServerPush(string url, string eventName) =>
            new ServerPushTrigger(url, eventName);

        internal static StartsWhen SignalR(string hubUrl, string method) =>
            new SignalRTrigger(hubUrl, method);
    }

    internal sealed class PageReadyTrigger : StartsWhen
    {
    }

    internal sealed class DocumentEventTrigger : StartsWhen
    {
        private readonly EventName _event;

        public string Event => _event.Value;

        internal DocumentEventTrigger(string eventName)
        {
            _event = EventName.Of(eventName);
        }
    }

    internal sealed class ComponentEventTrigger : StartsWhen
    {
        private readonly ComponentKey _component;
        private readonly EventName _event;

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
        private readonly EventName _event;

        public string Url => _url.Value;

        /// <summary>The SSE event name; messages sent without one are "message".</summary>
        public string Event => _event.Value;

        internal ServerPushTrigger(string url, string eventName)
        {
            _url = RequestUrl.Of(url);
            _event = EventName.Of(eventName);
        }
    }

    internal sealed class SignalRTrigger : StartsWhen
    {
        private readonly RequestUrl _hubUrl;
        private readonly MemberName _method;

        public string HubUrl => _hubUrl.Value;
        public string Method => _method.Value;

        internal SignalRTrigger(string hubUrl, string method)
        {
            _hubUrl = RequestUrl.Of(hubUrl);
            _method = MemberName.Of(method);
        }
    }
}
