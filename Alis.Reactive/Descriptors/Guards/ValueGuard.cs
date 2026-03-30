using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Sources;

namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// A leaf guard that evaluates a single condition: resolve the source value, coerce it,
    /// then apply the <see cref="Op"/> against an <see cref="Operand"/> or
    /// <see cref="RightSource"/>. Serialized as <c>kind: "value"</c> in the JSON plan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The left-hand side (<see cref="Source"/>) is always a <see cref="BindSource"/>
    /// pointing to a component read or event payload path. The right-hand side is either
    /// a literal <see cref="Operand"/> or a second <see cref="BindSource"/> for
    /// source-vs-source comparisons (e.g., rate.Value &lt;= budget.Value).
    /// </para>
    /// <para>
    /// Presence operators (<see cref="GuardOp.Truthy"/>, <see cref="GuardOp.IsNull"/>, etc.)
    /// have no operand: the operator tests the source value alone.
    /// </para>
    /// </remarks>
    public sealed class ValueGuard : Guard
    {
        /// <summary>Gets the type discriminator. Always <c>"value"</c>.</summary>
        [JsonPropertyOrder(-1)]
        public string Kind => "value";

        /// <summary>Gets the left-hand source whose resolved value is tested by the operator.</summary>
        public BindSource Source { get; }

        /// <summary>Gets the coercion type applied to the source value before comparison (e.g., <c>"number"</c>, <c>"date"</c>).</summary>
        public string CoerceAs { get; }

        /// <summary>Gets the operator constant from <see cref="GuardOp"/> that defines the comparison.</summary>
        public string Op { get; }

        /// <summary>Gets the literal right-hand operand, or <see langword="null"/> for unary operators and source-vs-source guards.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Operand { get; }

        /// <summary>
        /// Gets the right-hand source for source-vs-source comparisons. When present,
        /// the runtime resolves this source instead of using a literal <see cref="Operand"/>.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public BindSource? RightSource { get; }

        /// <summary>
        /// Gets the per-element coercion type for array operators such as
        /// <see cref="GuardOp.ArrayContains"/>. When the source is an array, this
        /// specifies how to coerce individual elements and the operand for
        /// type-consistent comparison.
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
