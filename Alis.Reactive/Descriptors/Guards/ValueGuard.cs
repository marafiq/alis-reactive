using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Descriptors.Guards
{
    public sealed class ValueGuard : Guard
    {
        [JsonPropertyOrder(-1)]
        public string Kind => "value";

        public BindSource Source { get; }
        public string CoerceAs { get; }
        public string Op { get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Operand { get; }

        public ValueGuard(BindSource source, string coerceAs, string op, object? operand = null)
        {
            Source = source;
            CoerceAs = coerceAs;
            Op = op;
            Operand = operand;
        }
    }
}
