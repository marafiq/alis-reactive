using System.Collections.Generic;

namespace Alis.Reactive.PlanModel
{
    internal sealed class Request
    {
        public string Method { get; }
        public string Url { get; }
        public string Container { get; set; }
        public RequestInput Input { get; set; }
        public List<Reaction> Before { get; set; }
        public List<ResponseHandler> Success { get; set; }
        public List<ResponseHandler> Error { get; set; }
        public List<Reaction> Complete { get; set; }
        public Request Next { get; set; }

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

        /// <summary>
        /// Validator type for deferred extraction at Render() time.
        /// Not serialized — used only during plan construction.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        internal System.Type ValidatorType { get; set; }
    }

    internal abstract class RequestInput
    {
        private protected RequestInput() { }
    }

    internal sealed class GatherInput : RequestInput
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

    internal sealed class ValueInput : RequestInput
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

    internal sealed class GatherField
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

    internal sealed class ResponseHandler
    {
        public int? Status { get; set; }
        public Reaction Reaction { get; }

        internal ResponseHandler(Reaction reaction)
        {
            Reaction = reaction;
        }
    }
}
