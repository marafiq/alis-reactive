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

        /// <summary>
        /// Element-level coercion for array operators (array-contains).
        /// When the source is an array, this specifies how to coerce individual elements
        /// and the operand for type-consistent comparison.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ElementCoerceAs { get; }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ValueGuard(BindSource source, string coerceAs, string op, object? operand = null)
        {
            Source = source;
            CoerceAs = coerceAs;
            Op = op;
            Operand = operand;
        }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ValueGuard(BindSource source, string coerceAs, string op, object? operand, string? elementCoerceAs)
        {
            Source = source;
            CoerceAs = coerceAs;
            Op = op;
            Operand = operand;
            ElementCoerceAs = elementCoerceAs;
        }

        /// <summary>
        /// NEVER make public. Constructed exclusively by framework builders. Public constructors
        /// on descriptor types allow devs to bypass the builder API and create invalid plan state.
        /// </summary>
        internal ValueGuard(BindSource left, string coerceAs, string op, BindSource right)
        {
            Source = left;
            CoerceAs = coerceAs;
            Op = op;
            RightSource = right;
        }
    }
}
