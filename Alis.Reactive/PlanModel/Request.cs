using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    public sealed class Request
    {
        public string Method { get; }
        public string Url { get; }
        public string? Container { get; internal set; }
        public RequestInput? Input { get; internal set; }
        public List<Reaction>? Before { get; internal set; }
        public List<ResponseHandler>? Success { get; internal set; }
        public List<ResponseHandler>? Error { get; internal set; }
        public List<Reaction>? Complete { get; internal set; }
        public Request? Next { get; internal set; }
        /// <summary>Custom HTTP headers to send with this request. Each value is a ValueProducer
        /// evaluated at request time — supports literals, component reads, and event args.</summary>
        public Dictionary<string, ValueProducer>? Headers { get; internal set; }
        /// <summary>URL template parameters. Each value is a ValueProducer evaluated at request time.
        /// Placeholders like {id} in the URL are replaced with the evaluated, URI-encoded values.</summary>
        public Dictionary<string, ValueProducer>? RouteParams { get; internal set; }

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

    public sealed class ValueInput : RequestInput
    {
        public string Kind => "value";
        public ValueProducer Value { get; }
        public string Transport { get; }

        internal ValueInput(ValueProducer value, string transport)
        {
            Value = value;
            Transport = transport;
        }
    }

    /// <summary>Maps an HTTP parameter name to a ValueProducer that reads the value at request time.
    /// Uses the shared evaluateValue path — same concept as pipeline reads and validation.</summary>
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

    public sealed class ResponseHandler
    {
        public int? Status { get; internal set; }
        public Reaction Reaction { get; }

        internal ResponseHandler(Reaction reaction)
        {
            Reaction = reaction;
        }
    }
}
