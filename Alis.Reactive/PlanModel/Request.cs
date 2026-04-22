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
        public string? Container { get; internal set; }
        /// <summary>Gets the request body strategy, or <see langword="null"/> for bodiless requests.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public RequestInput? Input { get; internal set; }
        /// <summary>Gets reactions to execute before the request is sent.</summary>
        public IReadOnlyList<Reaction> Before { get; internal set; } = System.Array.Empty<Reaction>();
        /// <summary>Gets the success response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Success { get; internal set; } = System.Array.Empty<ResponseHandler>();
        /// <summary>Gets the error response handlers.</summary>
        public IReadOnlyList<ResponseHandler> Error { get; internal set; } = System.Array.Empty<ResponseHandler>();
        /// <summary>Gets reactions to execute after the request completes regardless of outcome.</summary>
        public IReadOnlyList<Reaction> Complete { get; internal set; } = System.Array.Empty<Reaction>();
        /// <summary>Gets the chained follow-up request, or <see langword="null"/> if none.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Request? Next { get; internal set; }
        /// <summary>Gets the custom HTTP headers. Each value is evaluated at request time.</summary>
        public IReadOnlyDictionary<string, ValueProducer> Headers { get; internal set; } = new Dictionary<string, ValueProducer>();
        /// <summary>Gets the URL template parameters. Each value is evaluated and URI-encoded before replacing placeholders in the URL.</summary>
        public IReadOnlyDictionary<string, ValueProducer> RouteParams { get; internal set; } = new Dictionary<string, ValueProducer>();

        internal Request(string method, string url)
        {
            Method = method ?? throw new System.ArgumentNullException(nameof(method));
            Url = url ?? throw new System.ArgumentNullException(nameof(url));
        }

        internal static Request Get(string url) => new Request("GET", url);
        internal static Request Post(string url) => new Request("POST", url);
        internal static Request Put(string url) => new Request("PUT", url);
        internal static Request Delete(string url) => new Request("DELETE", url);
        internal static Request Patch(string url) => new Request("PATCH", url);

        [JsonIgnore]
        internal System.Type ValidatorType { get; set; }
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
        internal bool IncludeAll { get; set; }

        internal GatherInput(List<GatherField> components, string transport, ValueProducer? statics = null)
        {
            Components = components;
            Transport = transport;
            Statics = statics;
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
        public Shape Shape { get; }

        internal EventField(string key, string eventPath, Shape shape)
        {
            Key = key;
            EventPath = eventPath;
            Shape = shape ?? Shape.None;
        }
    }

    /// <summary>Maps an optional HTTP status code to a reaction.</summary>
    public sealed class ResponseHandler
    {
        /// <summary>Gets the HTTP status code to match, or <see langword="null"/> to match any status.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? Status { get; internal set; }
        /// <summary>Gets the reaction to execute when the status matches.</summary>
        public Reaction Reaction { get; }

        internal ResponseHandler(Reaction reaction)
        {
            Reaction = reaction;
        }
    }
}
