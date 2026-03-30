using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Requests
{
    public sealed class StaticGather : GatherItem
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "static";

        public string Param { get; }
        public object Value { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal StaticGather(string param, object value)
        {
            Param = param;
            Value = value;
        }
    }
}
