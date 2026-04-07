using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    /// <summary>
    /// Entry point for building composed field conditions inside WhenFields().
    /// </summary>
    public sealed class FieldConditionBuilder<T> where T : class
    {
        internal FieldConditionBuilder() { }

        /// <summary>Start a condition on a field — chain an operator to complete it.</summary>
        public FieldStart<T, TProp> Field<TProp>(Expression<Func<T, TProp>> field) =>
            new FieldStart<T, TProp>(field);
    }

    /// <summary>
    /// Intermediate builder — a field has been selected, now pick an operator.
    /// No IComparable constraints — comparison is deferred to the TS runtime
    /// (matching ConditionSourceBuilder pattern).
    /// </summary>
    public sealed class FieldStart<T, TProp> where T : class
    {
        private readonly string _fieldName;
        private readonly Func<T, TProp> _fieldFunc;

        internal FieldStart(Expression<Func<T, TProp>> field)
        {
            _fieldName = ReactiveValidator<T>.ExtractPropertyName(field);
            _fieldFunc = field.Compile();
        }

        // ── Equality ───────────────────────────────────────────────────────

        public FieldGuard<T> Truthy() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "truthy"),
                x => !IsFalsy(_fieldFunc(x)));

        public FieldGuard<T> Falsy() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "falsy"),
                x => IsFalsy(_fieldFunc(x)));

        private static bool IsFalsy(TProp value) => value switch
        {
            null => true,
            false => true,
            "" => true,
            0 => true,
            0L => true,
            0m => true,
            0d => true,
            0f => true,
            _ => false
        };

        public FieldGuard<T> Eq(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "eq", ReactiveValidator<T>.SerializeConditionValue(value)),
                x => Equals(_fieldFunc(x), value));

        public FieldGuard<T> Neq(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "neq", ReactiveValidator<T>.SerializeConditionValue(value)),
                x => !Equals(_fieldFunc(x), value));

        // ── Ordering ───────────────────────────────────────────────────────

        public FieldGuard<T> Gt(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "gt", ReactiveValidator<T>.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) > 0);

        public FieldGuard<T> Gte(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "gte", ReactiveValidator<T>.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) >= 0);

        public FieldGuard<T> Lt(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "lt", ReactiveValidator<T>.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) < 0);

        public FieldGuard<T> Lte(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "lte", ReactiveValidator<T>.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) <= 0);

        // ── Presence ───────────────────────────────────────────────────────

        public FieldGuard<T> IsNull() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "is-null"),
                x => _fieldFunc(x) == null);

        public FieldGuard<T> NotNull() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "not-null"),
                x => _fieldFunc(x) != null);

        public FieldGuard<T> IsEmpty() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "is-empty"),
                x => _fieldFunc(x) is string s ? string.IsNullOrEmpty(s) : _fieldFunc(x) == null);

        public FieldGuard<T> NotEmpty() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "not-empty"),
                x => _fieldFunc(x) is string s ? !string.IsNullOrEmpty(s) : _fieldFunc(x) != null);

        // ── Membership ─────────────────────────────────────────────────────

        public FieldGuard<T> In(params TProp[] values)
        {
            var serialized = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
                serialized[i] = ReactiveValidator<T>.SerializeConditionValue(values[i]);

            var set = new HashSet<TProp>(values);
            return new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "in", serialized),
                x => set.Contains(_fieldFunc(x)));
        }

        // ── Text ───────────────────────────────────────────────────────────

        public FieldGuard<T> Contains(string substring) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "contains", substring),
                x => (_fieldFunc(x) as string)?.Contains(substring) == true);

        public FieldGuard<T> StartsWith(string prefix) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "starts-with", prefix),
                x => (_fieldFunc(x) as string)?.StartsWith(prefix) == true);

        public FieldGuard<T> EndsWith(string suffix) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, "ends-with", suffix),
                x => (_fieldFunc(x) as string)?.EndsWith(suffix) == true);
    }

    /// <summary>
    /// A completed field condition with its server predicate.
    /// Chain .And(), .Or(), .Not() to compose multiple conditions.
    /// </summary>
    public sealed class FieldGuard<T> where T : class
    {
        internal FieldCondition Condition { get; }
        internal Func<T, bool> ServerPredicate { get; }

        internal FieldGuard(FieldCondition condition, Func<T, bool> serverPredicate)
        {
            Condition = condition;
            ServerPredicate = serverPredicate;
        }

        /// <summary>Logical AND — both this and the other condition must be true.</summary>
        public FieldGuard<T> And(FieldGuard<T> other) =>
            new FieldGuard<T>(
                FieldCondition.All(Condition, other.Condition),
                x => ServerPredicate(x) && other.ServerPredicate(x));

        /// <summary>Logical OR — either this or the other condition must be true.</summary>
        public FieldGuard<T> Or(FieldGuard<T> other) =>
            new FieldGuard<T>(
                FieldCondition.Any(Condition, other.Condition),
                x => ServerPredicate(x) || other.ServerPredicate(x));

        /// <summary>Logical NOT — inverts this condition.</summary>
        public FieldGuard<T> Not() =>
            new FieldGuard<T>(
                FieldCondition.Not(Condition),
                x => !ServerPredicate(x));
    }
}
