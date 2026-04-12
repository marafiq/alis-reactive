using System;
using System.Collections.Generic;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// A symbolic condition tree built at extraction time using field NAMES.
    /// Resolved to <see cref="PlanModel.Condition"/> at render time when
    /// the component map is available.
    /// </summary>
    public abstract class FieldCondition
    {
        private protected FieldCondition() { }

        internal static FieldCondition Compare(string field, string op, object? value = null) =>
            new FieldCompare(field, op, value);

        internal static FieldCondition All(params FieldCondition[] terms) =>
            new FieldAll(terms);

        internal static FieldCondition Any(params FieldCondition[] terms) =>
            new FieldAny(terms);

        internal static FieldCondition Not(FieldCondition term) =>
            new FieldNot(term);
    }

    /// <summary>
    /// A single field comparison: read field, apply operator, compare to optional value.
    /// </summary>
    public sealed class FieldCompare : FieldCondition
    {
        /// <summary>Property name to check (e.g. "IsEmployed").</summary>
        public string Field { get; }

        /// <summary>Operator from <see cref="PlanModel.CompareOp"/>.</summary>
        public string Op { get; }

        /// <summary>Comparison value (null for unary operators like truthy/falsy).</summary>
        public object? Value { get; }

        internal FieldCompare(string field, string op, object? value)
        {
            Field = field ?? throw new ArgumentNullException(nameof(field));
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Value = value;
        }
    }

    /// <summary>Logical AND — all terms must be true.</summary>
    public sealed class FieldAll : FieldCondition
    {
        /// <summary>Gets the child conditions that must all be true.</summary>
        public IReadOnlyList<FieldCondition> Terms { get; }

        internal FieldAll(FieldCondition[] terms)
        {
            Terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    /// <summary>Logical OR — any term must be true.</summary>
    public sealed class FieldAny : FieldCondition
    {
        /// <summary>Gets the child conditions where at least one must be true.</summary>
        public IReadOnlyList<FieldCondition> Terms { get; }

        internal FieldAny(FieldCondition[] terms)
        {
            Terms = terms ?? throw new ArgumentNullException(nameof(terms));
        }
    }

    /// <summary>Logical NOT — inverts the inner term.</summary>
    public sealed class FieldNot : FieldCondition
    {
        /// <summary>Gets the inner condition to negate.</summary>
        public FieldCondition Term { get; }

        internal FieldNot(FieldCondition term)
        {
            Term = term ?? throw new ArgumentNullException(nameof(term));
        }
    }
}
