using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;
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
            _fieldName = FieldConditionHelpers.ExtractPropertyName(field);
            _fieldFunc = field.Compile();
        }

        // ── Equality ───────────────────────────────────────────────────────

        /// <summary>
        /// Generic truthy check: treats <see langword="null"/>, <see langword="false"/>, 0
        /// (all numeric types), and empty string as falsy.
        /// For bool-specific overload, use <c>WhenField(Expression&lt;Func&lt;T, bool&gt;&gt;)</c> directly.
        /// </summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Truthy() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Truthy),
                x => !IsFalsy(_fieldFunc(x)));

        /// <summary>
        /// Generic falsy check: treats <see langword="null"/>, <see langword="false"/>, 0
        /// (all numeric types), and empty string as falsy.
        /// </summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Falsy() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Falsy),
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

        /// <summary>Tests whether the field equals the specified value.</summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Eq(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Eq, FieldConditionHelpers.SerializeConditionValue(value)),
                x => Equals(_fieldFunc(x), value));

        /// <summary>Tests whether the field does not equal the specified value.</summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Neq(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Neq, FieldConditionHelpers.SerializeConditionValue(value)),
                x => !Equals(_fieldFunc(x), value));

        // ── Ordering ───────────────────────────────────────────────────────

        /// <summary>Tests whether the field is greater than the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Gt(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Gt, FieldConditionHelpers.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) > 0);

        /// <summary>Tests whether the field is greater than or equal to the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Gte(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Gte, FieldConditionHelpers.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) >= 0);

        /// <summary>Tests whether the field is less than the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Lt(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Lt, FieldConditionHelpers.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) < 0);

        /// <summary>Tests whether the field is less than or equal to the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Lte(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Lte, FieldConditionHelpers.SerializeConditionValue(value)),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), value) <= 0);

        // ── Presence ───────────────────────────────────────────────────────

        /// <summary>Tests whether the field value is <see langword="null"/>.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> IsNull() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.IsNull),
                x => _fieldFunc(x) == null);

        /// <summary>Tests whether the field value is not <see langword="null"/>.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> NotNull() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.NotNull),
                x => _fieldFunc(x) != null);

        /// <summary>Tests whether the field value is <see langword="null"/> or an empty string.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> IsEmpty() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.IsEmpty),
                x => _fieldFunc(x) is string s ? string.IsNullOrEmpty(s) : _fieldFunc(x) == null);

        /// <summary>Tests whether the field value is not <see langword="null"/> and not an empty string.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> NotEmpty() =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.NotEmpty),
                x => _fieldFunc(x) is string s ? !string.IsNullOrEmpty(s) : _fieldFunc(x) != null);

        // ── Membership ─────────────────────────────────────────────────────

        /// <summary>Tests whether the field value is one of the specified values.</summary>
        /// <param name="values">The allowed values.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> In(params TProp[] values)
        {
            var serialized = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
                serialized[i] = FieldConditionHelpers.SerializeConditionValue(values[i]);

            var set = new HashSet<TProp>(values);
            return new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.In, serialized),
                x => set.Contains(_fieldFunc(x)));
        }

        /// <summary>Tests whether the field value is not one of the specified values.</summary>
        /// <param name="values">The disallowed values.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> NotIn(params TProp[] values)
        {
            var serialized = new object[values.Length];
            for (int i = 0; i < values.Length; i++)
                serialized[i] = FieldConditionHelpers.SerializeConditionValue(values[i]);

            var set = new HashSet<TProp>(values);
            return new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.NotIn, serialized),
                x => !set.Contains(_fieldFunc(x)));
        }

        /// <summary>Tests whether the field value falls within the inclusive range [low, high].</summary>
        /// <param name="low">The lower bound (inclusive).</param>
        /// <param name="high">The upper bound (inclusive).</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Between(TProp low, TProp high)
        {
            var serialized = new object[]
            {
                FieldConditionHelpers.SerializeConditionValue(low),
                FieldConditionHelpers.SerializeConditionValue(high)
            };
            return new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Between, serialized),
                x => Comparer<TProp>.Default.Compare(_fieldFunc(x), low) >= 0 &&
                     Comparer<TProp>.Default.Compare(_fieldFunc(x), high) <= 0);
        }

        // ── Text ───────────────────────────────────────────────────────────

        /// <summary>Tests whether the string field contains the specified substring.</summary>
        /// <param name="substring">The substring to search for.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Contains(string substring) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Contains, substring),
                x => (_fieldFunc(x) as string)?.Contains(substring) == true);

        /// <summary>Tests whether the string field starts with the specified prefix.</summary>
        /// <param name="prefix">The prefix to check for.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> StartsWith(string prefix) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.StartsWith, prefix),
                x => (_fieldFunc(x) as string)?.StartsWith(prefix) == true);

        /// <summary>Tests whether the string field ends with the specified suffix.</summary>
        /// <param name="suffix">The suffix to check for.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> EndsWith(string suffix) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.EndsWith, suffix),
                x => (_fieldFunc(x) as string)?.EndsWith(suffix) == true);

        /// <summary>Tests whether the string field matches the specified regular expression.</summary>
        /// <param name="pattern">The regular expression pattern.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Matches(string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            return new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.Matches, pattern),
                x => (_fieldFunc(x) as string) is { } s && regex.IsMatch(s));
        }

        /// <summary>Tests whether the string field has at least the specified number of characters.</summary>
        /// <param name="minLength">The minimum character count.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> MinLength(int minLength) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.MinLength, minLength),
                x => (_fieldFunc(x) as string) is { } s && s.Length >= minLength);

        // ── Array ──────────────────────────────────────────────────────────

        /// <summary>Tests whether the array field contains the specified element.</summary>
        /// <param name="value">The element to search for in the array.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> ArrayContains(TProp value) =>
            new FieldGuard<T>(
                FieldCondition.Compare(_fieldName, CompareOp.ArrayContains,
                    FieldConditionHelpers.SerializeConditionValue(value)),
                x => _fieldFunc(x) is System.Collections.IEnumerable enumerable &&
                     enumerable.Cast<object>().Contains(value));
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
