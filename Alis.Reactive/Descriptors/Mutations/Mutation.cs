using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Mutations
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Mutation>))]
    public abstract class Mutation { }

    public sealed class SetPropMutation : Mutation
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "set-prop";

        public string Prop { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Coerce { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal SetPropMutation(string prop, string? coerce = null)
        {
            Prop = prop;
            Coerce = coerce;
        }
    }

    public sealed class CallMutation : Mutation
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "call";

        public string Method { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Chain { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MethodArg[]? Args { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal CallMutation(string method, string? chain = null, MethodArg[]? args = null)
        {
            Method = method;
            Chain = chain;
            Args = args;
        }
    }
}
