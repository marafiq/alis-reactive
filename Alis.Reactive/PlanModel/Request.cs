using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    public sealed class Request
    {
        public string Method { get; }
        public string Url { get; }
        public string Container { get; internal set; }
        public RequestInput Input { get; internal set; }
        public List<Reaction> Before { get; internal set; }
        public List<ResponseHandler> Success { get; internal set; }
        public List<ResponseHandler> Error { get; internal set; }
        public List<Reaction> Complete { get; internal set; }
        public Request Next { get; internal set; }

        internal Request(string method, string url)
        {
            Method = method;
            Url = url;
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

    public sealed class GatherInput : RequestInput
    {
        public string Kind => "gather";
        public List<GatherField> Components { get; }
        public string Transport { get; }

        internal GatherInput(List<GatherField> components, string transport)
        {
            Components = components;
            Transport = transport;
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

    public sealed class GatherField
    {
        public string Component { get; }
        public string Key { get; }

        internal GatherField(string component, string key)
        {
            Component = component;
            Key = key;
        }

        internal static GatherField Of(string component, string key) => new GatherField(component, key);
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
