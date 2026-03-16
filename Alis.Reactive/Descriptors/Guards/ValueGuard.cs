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

        /// <summary>
        /// When present, the right-hand side is a source (component or event),
        /// not a literal operand. Enables source-vs-source: rate.Value &lt;= budget.Value.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BindSource? RightSource { get; }

        public ValueGuard(BindSource source, string coerceAs, string op, object? operand = null)
        {
            Source = source;
            CoerceAs = coerceAs;
            Op = op;
            Operand = operand;
        }

        /// <summary>Source-vs-source constructor.</summary>
        public ValueGuard(BindSource left, string coerceAs, string op, BindSource right)
        {
            Source = left;
            CoerceAs = coerceAs;
            Op = op;
            RightSource = right;
        }
    }
}
