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
        private readonly SelectedClientValidationField<T, TProp> _field;

        internal FieldStart(Expression<Func<T, TProp>> field)
        {
            _field = SelectedClientValidationField<T, TProp>.From(field);
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
            _field.Guard(CompareOperator.Truthy, value => !IsFalsy(value));

        /// <summary>
        /// Generic falsy check: treats <see langword="null"/>, <see langword="false"/>, 0
        /// (all numeric types), and empty string as falsy.
        /// </summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Falsy() =>
            _field.Guard(CompareOperator.Falsy, value => IsFalsy(value));

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
            _field.GuardAgainstLiteral(
                CompareOperator.Eq,
                value,
                candidate => Equals(candidate, value));

        /// <summary>Tests whether the field does not equal the specified value.</summary>
        /// <param name="value">The value to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Neq(TProp value) =>
            _field.GuardAgainstLiteral(
                CompareOperator.Neq,
                value,
                candidate => !Equals(candidate, value));

        // ── Ordering ───────────────────────────────────────────────────────

        /// <summary>Tests whether the field is greater than the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Gt(TProp value)
        {
            var threshold = FieldConditionThreshold<TProp>.At(value);
            return _field.GuardAgainstLiteral(
                CompareOperator.Gt,
                value,
                candidate => threshold.AcceptsGreaterValue(candidate));
        }

        /// <summary>Tests whether the field is greater than or equal to the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Gte(TProp value)
        {
            var threshold = FieldConditionThreshold<TProp>.At(value);
            return _field.GuardAgainstLiteral(
                CompareOperator.Gte,
                value,
                candidate => threshold.AcceptsGreaterOrEqualValue(candidate));
        }

        /// <summary>Tests whether the field is less than the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Lt(TProp value)
        {
            var threshold = FieldConditionThreshold<TProp>.At(value);
            return _field.GuardAgainstLiteral(
                CompareOperator.Lt,
                value,
                candidate => threshold.AcceptsLowerValue(candidate));
        }

        /// <summary>Tests whether the field is less than or equal to the specified value.</summary>
        /// <param name="value">The threshold to compare against.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Lte(TProp value)
        {
            var threshold = FieldConditionThreshold<TProp>.At(value);
            return _field.GuardAgainstLiteral(
                CompareOperator.Lte,
                value,
                candidate => threshold.AcceptsLowerOrEqualValue(candidate));
        }

        // ── Presence ───────────────────────────────────────────────────────

        /// <summary>Tests whether the field value is <see langword="null"/>.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> IsNull() =>
            _field.Guard(CompareOperator.IsNull, value => value == null);

        /// <summary>Tests whether the field value is not <see langword="null"/>.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> NotNull() =>
            _field.Guard(CompareOperator.NotNull, value => value != null);

        /// <summary>Tests whether the field value is <see langword="null"/> or an empty string.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> IsEmpty() =>
            _field.Guard(
                CompareOperator.IsEmpty,
                value => FieldConditionPredicates.IsEmptyValue(value));

        /// <summary>Tests whether the field value is not <see langword="null"/> and not an empty string.</summary>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> NotEmpty() =>
            _field.Guard(
                CompareOperator.NotEmpty,
                value => FieldConditionPredicates.HasNonEmptyValue(value));

        // ── Membership ─────────────────────────────────────────────────────

        /// <summary>Tests whether the field value is one of the specified values.</summary>
        /// <param name="values">The allowed values.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> In(params TProp[] values)
        {
            var set = FieldConditionValueSet<TProp>.Of(values);
            return _field.GuardWithOperand(
                CompareOperator.In,
                FieldComparisonValue.Array(set.ValuesForPlan),
                value => set.Contains(value));
        }

        /// <summary>Tests whether the field value is not one of the specified values.</summary>
        /// <param name="values">The disallowed values.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> NotIn(params TProp[] values)
        {
            var set = FieldConditionValueSet<TProp>.Of(values);
            return _field.GuardWithOperand(
                CompareOperator.NotIn,
                FieldComparisonValue.Array(set.ValuesForPlan),
                value => set.DoesNotContain(value));
        }

        /// <summary>Tests whether the field value falls within the inclusive range [low, high].</summary>
        /// <param name="low">The lower bound (inclusive).</param>
        /// <param name="high">The upper bound (inclusive).</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Between(TProp low, TProp high)
        {
            var range = FieldConditionRange<TProp>.Inclusive(low, high);
            return _field.GuardWithOperand(
                CompareOperator.Between,
                FieldComparisonValue.Array(range.ValuesForPlan),
                value => range.Contains(value));
        }

        // ── Text ───────────────────────────────────────────────────────────

        /// <summary>Tests whether the string field contains the specified substring.</summary>
        /// <param name="substring">The substring to search for.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Contains(string substring) =>
            _field.GuardAgainstLiteral(
                CompareOperator.Contains,
                substring,
                value => FieldConditionPredicates.TextContains(value, substring));

        /// <summary>Tests whether the string field starts with the specified prefix.</summary>
        /// <param name="prefix">The prefix to check for.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> StartsWith(string prefix) =>
            _field.GuardAgainstLiteral(
                CompareOperator.StartsWith,
                prefix,
                value => FieldConditionPredicates.TextStartsWith(value, prefix));

        /// <summary>Tests whether the string field ends with the specified suffix.</summary>
        /// <param name="suffix">The suffix to check for.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> EndsWith(string suffix) =>
            _field.GuardAgainstLiteral(
                CompareOperator.EndsWith,
                suffix,
                value => FieldConditionPredicates.TextEndsWith(value, suffix));

        /// <summary>Tests whether the string field matches the specified regular expression.</summary>
        /// <param name="pattern">The regular expression pattern.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> Matches(string pattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(pattern);
            return _field.GuardAgainstLiteral(
                CompareOperator.Matches,
                pattern,
                value => FieldConditionPredicates.TextMatches(value, regex));
        }

        /// <summary>Tests whether the string field has at least the specified number of characters.</summary>
        /// <param name="minLength">The minimum character count.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> MinLength(int minLength)
        {
            var minimumLength = Alis.Reactive.PlanModel.MinimumTextLength.From(minLength, nameof(minLength));
            return _field.GuardAgainstLiteral(
                CompareOperator.MinLength,
                minimumLength.Value,
                value => FieldConditionPredicates.TextHasMinimumLength(value, minimumLength));
        }

        // ── Array ──────────────────────────────────────────────────────────

        /// <summary>Tests whether the array field contains the specified element.</summary>
        /// <param name="value">The element to search for in the array.</param>
        /// <returns>A guard that can be composed with <see cref="FieldGuard{T}.And"/>,
        /// <see cref="FieldGuard{T}.Or"/>, or <see cref="FieldGuard{T}.Not"/>.</returns>
        public FieldGuard<T> ArrayContains(TProp value) =>
            _field.GuardAgainstCollectionItem(
                CompareOperator.ArrayContains,
                value,
                candidate => FieldConditionPredicates.ArrayContains(candidate, value));
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
        public FieldGuard<T> And(FieldGuard<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            bool BothConditionsPass(T model)
            {
                return ServerPredicate(model) && other.ServerPredicate(model);
            }

            return new FieldGuard<T>(
                FieldCondition.All(Condition, other.Condition),
                BothConditionsPass);
        }

        /// <summary>Logical OR — either this or the other condition must be true.</summary>
        public FieldGuard<T> Or(FieldGuard<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            bool EitherConditionPasses(T model)
            {
                return ServerPredicate(model) || other.ServerPredicate(model);
            }

            return new FieldGuard<T>(
                FieldCondition.Any(Condition, other.Condition),
                EitherConditionPasses);
        }

        /// <summary>Logical NOT — inverts this condition.</summary>
        public FieldGuard<T> Not() =>
            new FieldGuard<T>(
                FieldCondition.Not(Condition),
                x => !ServerPredicate(x));
    }

    internal static class FieldConditionPredicates
    {
        internal static bool IsEmptyValue<TValue>(TValue value)
        {
            if (value == null) return true;
            if (value is string text) return string.IsNullOrEmpty(text);
            return false;
        }

        internal static bool HasNonEmptyValue<TValue>(TValue value) =>
            !IsEmptyValue(value);

        internal static bool ArrayContains<TItem>(object? value, TItem expected)
        {
            if (!(value is IEnumerable enumerable)) return false;
            foreach (var item in enumerable)
            {
                if (Equals(item, expected)) return true;
            }

            return false;
        }

        internal static bool TextContains(object? value, string substring) =>
            value is string text && text.Contains(substring);

        internal static bool TextStartsWith(object? value, string prefix) =>
            value is string text && text.StartsWith(prefix);

        internal static bool TextEndsWith(object? value, string suffix) =>
            value is string text && text.EndsWith(suffix);

        internal static bool TextMatches(object? value, System.Text.RegularExpressions.Regex regex)
        {
            if (regex == null) throw new ArgumentNullException(nameof(regex));
            return value is string text && regex.IsMatch(text);
        }

        internal static bool TextHasMinimumLength(object? value, MinimumTextLength minLength)
        {
            if (minLength == null) throw new ArgumentNullException(nameof(minLength));
            return value is string text && text.Length >= minLength.Value;
        }

        internal static bool EnumerableContains<TItem>(IEnumerable<TItem>? values, TItem expected)
        {
            if (values == null) return false;
            return values.Contains(expected);
        }
    }

    internal sealed class FieldConditionValueSet<TValue>
    {
        private readonly HashSet<TValue> _values;

        private FieldConditionValueSet(TValue[] values)
        {
            _values = new HashSet<TValue>(values);
            ValuesForPlan = Serialize(values);
        }

        internal IReadOnlyList<object?> ValuesForPlan { get; }

        internal static FieldConditionValueSet<TValue> Of(TValue[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return new FieldConditionValueSet<TValue>(values);
        }

        internal bool Contains(TValue value) => _values.Contains(value);

        internal bool DoesNotContain(TValue value) => !_values.Contains(value);

        private static IReadOnlyList<object?> Serialize(TValue[] values)
        {
            var planValues = new List<object?>(values.Length);
            foreach (var value in values)
                planValues.Add(ValidationConditionLiteral.From(value));
            return planValues;
        }
    }

    internal sealed class FieldConditionRange<TValue>
    {
        private readonly TValue _low;
        private readonly TValue _high;

        private FieldConditionRange(TValue low, TValue high)
        {
            _low = low;
            _high = high;
            ValuesForPlan = new[]
            {
                ValidationConditionLiteral.From(low),
                ValidationConditionLiteral.From(high)
            };
        }

        internal IReadOnlyList<object?> ValuesForPlan { get; }

        internal static FieldConditionRange<TValue> Inclusive(TValue low, TValue high) =>
            new FieldConditionRange<TValue>(low, high);

        internal bool Contains(TValue value)
        {
            var isAtOrAboveLow = Comparer<TValue>.Default.Compare(value, _low) >= 0;
            var isAtOrBelowHigh = Comparer<TValue>.Default.Compare(value, _high) <= 0;
            return isAtOrAboveLow && isAtOrBelowHigh;
        }
    }

    internal sealed class FieldConditionThreshold<TValue>
    {
        private readonly TValue _value;

        private FieldConditionThreshold(TValue value)
        {
            _value = value;
        }

        internal static FieldConditionThreshold<TValue> At(TValue value) =>
            new FieldConditionThreshold<TValue>(value);

        internal bool AcceptsGreaterValue(TValue candidate) =>
            Compare(candidate) > 0;

        internal bool AcceptsGreaterOrEqualValue(TValue candidate) =>
            Compare(candidate) >= 0;

        internal bool AcceptsLowerValue(TValue candidate) =>
            Compare(candidate) < 0;

        internal bool AcceptsLowerOrEqualValue(TValue candidate) =>
            Compare(candidate) <= 0;

        private int Compare(TValue candidate) =>
            Comparer<TValue>.Default.Compare(candidate, _value);
    }
}
