using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Sources;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Descriptors.Mutations
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<MethodArg>))]
    public abstract class MethodArg { }

    public sealed class LiteralArg : MethodArg
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "literal";

        public object Value { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal LiteralArg(object value)
        {
            Value = value;
        }
    }

    public sealed class SourceArg : MethodArg
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "source";

        public BindSource Source { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Coerce { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal SourceArg(BindSource source, string? coerce = null)
        {
            Source = source;
            Coerce = coerce;
        }
    }
}
