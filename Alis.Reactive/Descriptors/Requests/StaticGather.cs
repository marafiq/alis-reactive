using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    public sealed class StaticGather : GatherItem
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "static";

        public string Param { get; }
        public object Value { get; }

        public StaticGather(string param, object value)
        {
            Param = param;
            Value = value;
        }
    }
}
