using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>An HTTP request definition in the reactive plan.</summary>
    public sealed class Request
    {
        /// <summary>Gets the HTTP method (GET, POST, PUT, DELETE, PATCH).</summary>
        public string Method { get; }
        /// <summary>Gets the request URL, which may contain template placeholders like <c>{id}</c>.</summary>
        public string Url { get; }
        /// <summary>Gets the DOM element ID where validation errors are displayed, or <see langword="null"/> when validation is not configured for this request.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Container { get; }
        /// <summary>Gets the request body strategy, or <see langword="null"/> for bodiless requests.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RequestInput? Input { get; }
        /// <summary>Gets reactions to execute before the request is sent.</summary>
        public IReadOnlyList<Reaction> Before { get; }
        /// <summary>Gets the success response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Success { get; }
        /// <summary>Gets the error response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Error { get; }
        /// <summary>Gets reactions to execute after the request completes regardless of outcome.</summary>
        public IReadOnlyList<Reaction> Complete { get; }
        /// <summary>Gets the chained follow-up request, or <see langword="null"/> if none.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Request? Next { get; }
        /// <summary>Gets the custom HTTP headers. Each value is evaluated at request time.</summary>
        public IReadOnlyDictionary<string, ValueProducer> Headers { get; }
        /// <summary>Gets the URL template parameters. Each value is evaluated and URI-encoded before replacing placeholders in the URL.</summary>
        public IReadOnlyDictionary<string, ValueProducer> RouteParams { get; }

        internal Request(
            string method,
            string url,
            string? container = null,
            RequestInput? input = null,
            IReadOnlyList<Reaction>? before = null,
            IReadOnlyList<ResponseHandler>? success = null,
            IReadOnlyList<ResponseHandler>? error = null,
            IReadOnlyList<Reaction>? complete = null,
            Request? next = null,
            IReadOnlyDictionary<string, ValueProducer>? headers = null,
            IReadOnlyDictionary<string, ValueProducer>? routeParams = null)
        {
            Method = method ?? throw new System.ArgumentNullException(nameof(method));
            Url = url ?? throw new System.ArgumentNullException(nameof(url));
            Container = container;
            Input = input;
            Before = before ?? System.Array.Empty<Reaction>();
            Success = success ?? System.Array.Empty<ResponseHandler>();
            Error = error ?? System.Array.Empty<ResponseHandler>();
            Complete = complete ?? System.Array.Empty<Reaction>();
            Next = next;
            Headers = headers ?? new Dictionary<string, ValueProducer>();
            RouteParams = routeParams ?? new Dictionary<string, ValueProducer>();
        }

        /// <summary>Returns a copy of this request with <see cref="Before"/> replaced.</summary>
        internal Request WithBefore(IReadOnlyList<Reaction> before) =>
            new Request(Method, Url, Container, Input, before, Success, Error,
                Complete, Next, Headers, RouteParams);
    }

    /// <summary>Base class for request body strategies. Not constructed in application code.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<RequestInput>))]
    public abstract class RequestInput
    {
        private protected RequestInput() { }
    }

    internal sealed class GatherInput : RequestInput
    {
        public string Kind => "gather";
        public List<GatherField> Components { get; }
        public string Transport { get; }
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public ValueProducer? Statics { get; }

        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        [System.Text.Json.Serialization.JsonInclude]
        internal bool IncludeAll { get; }

        internal GatherInput(List<GatherField> components, string transport,
            ValueProducer? statics = null, bool includeAll = false)
        {
            Components = components;
            Transport = transport;
            Statics = statics;
            IncludeAll = includeAll;
        }
    }

    /// <summary>Sends a single evaluated value as the request body.</summary>
    public sealed class ValueInput : RequestInput
    {
        /// <summary>Gets the kind. Always <c>"value"</c>.</summary>
        public string Kind => "value";
        /// <summary>Gets the value expression to send as the body.</summary>
        public ValueProducer Value { get; }
        /// <summary>Gets the transport format (json or form).</summary>
        public string Transport { get; }

        internal ValueInput(ValueProducer value, string transport)
        {
            Value = value;
            Transport = transport;
        }
    }

    /// <summary>Maps an HTTP parameter name to a value expression evaluated at request time.</summary>
    internal sealed class GatherField
    {
        /// <summary>HTTP parameter name (from model binding path or explicit override).</summary>
        public string Key { get; }
        /// <summary>How to read the value. Carries source, member, and shape.</summary>
        public ValueProducer Value { get; }

        internal GatherField(string key, ValueProducer value)
        {
            Key = key;
            Value = value;
        }

        internal static GatherField Of(string key, ValueProducer value)
            => new GatherField(key, value);
    }

    /// <summary>
    /// A literal key/value pair included in a gather request.
    /// Replaces the "$static:" magic-string prefix.
    /// </summary>
    internal sealed class StaticField
    {
        public string Key { get; }
        public object Value { get; }

        internal StaticField(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }

    /// <summary>
    /// A value read from the triggering event's payload.
    /// Replaces the "$event:" magic-string prefix.
    /// </summary>
    internal sealed class EventField
    {
        public string Key { get; }
        public string EventPath { get; }

        internal EventField(string key, string eventPath)
        {
            Key = key;
            EventPath = eventPath;
        }
    }

    /// <summary>Maps an optional HTTP status code to a reaction.</summary>
    public sealed class ResponseHandler
    {
        /// <summary>Gets the HTTP status code to match, or <see langword="null"/> to match any status.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Status { get; }
        /// <summary>Gets the reaction to execute when the status matches.</summary>
        public Reaction Reaction { get; }

        internal ResponseHandler(Reaction reaction, int? status = null)
        {
            Reaction = reaction ?? throw new System.ArgumentNullException(nameof(reaction));
            Status = status;
        }
    }
}
