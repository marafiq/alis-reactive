using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    public sealed class FieldConditionBuilder<T> where T : class
    {
        internal FieldConditionBuilder() { }

        public FieldStart<T, TProp> Field<TProp>(Expression<Func<T, TProp>> field) =>
            new FieldStart<T, TProp>(field);
    }

    public sealed class FieldStart<T, TProp> where T : class
    {
        private readonly SelectedClientValidationField<T, TProp> _field;

        internal FieldStart(Expression<Func<T, TProp>> field)
        {
            _field = SelectedClientValidationField<T, TProp>.From(field);
        }

        public FieldGuard<T> Truthy() => Unary(CompareOperator.Truthy, value => !IsFalsy(value));
        public FieldGuard<T> Falsy() => Unary(CompareOperator.Falsy, IsFalsy);
        public FieldGuard<T> Eq(TProp value) => Literal(CompareOperator.Eq, value, candidate => Equals(candidate, value));
        public FieldGuard<T> Neq(TProp value) => Literal(CompareOperator.Neq, value, candidate => !Equals(candidate, value));
        public FieldGuard<T> Gt(TProp value) => Compare(CompareOperator.Gt, value, result => result > 0);
        public FieldGuard<T> Gte(TProp value) => Compare(CompareOperator.Gte, value, result => result >= 0);
        public FieldGuard<T> Lt(TProp value) => Compare(CompareOperator.Lt, value, result => result < 0);
        public FieldGuard<T> Lte(TProp value) => Compare(CompareOperator.Lte, value, result => result <= 0);
        public FieldGuard<T> IsNull() => Unary(CompareOperator.IsNull, value => value == null);
        public FieldGuard<T> NotNull() => Unary(CompareOperator.NotNull, value => value != null);
        public FieldGuard<T> IsEmpty() => Unary(CompareOperator.IsEmpty, IsEmptyValue);
        public FieldGuard<T> NotEmpty() => Unary(CompareOperator.NotEmpty, value => !IsEmptyValue(value));
        public FieldGuard<T> Contains(string substring) => Text(CompareOperator.Contains, substring, text => text.Contains(substring));
        public FieldGuard<T> StartsWith(string prefix) => Text(CompareOperator.StartsWith, prefix, text => text.StartsWith(prefix));
        public FieldGuard<T> EndsWith(string suffix) => Text(CompareOperator.EndsWith, suffix, text => text.EndsWith(suffix));

        public FieldGuard<T> Matches(string pattern)
        {
            var regex = new Regex(pattern);
            return Text(CompareOperator.Matches, pattern, text => regex.IsMatch(text));
        }

        public FieldGuard<T> MinLength(int minLength)
        {
            var minimum = MinimumTextLength.From(minLength, nameof(minLength));
            return Literal(
                CompareOperator.MinLength,
                minimum.Value,
                value => value is string text && text.Length >= minimum.Value);
        }

        public FieldGuard<T> In(params TProp[] values)
        {
            var set = ValueSet(values);
            return _field.GuardWithOperand(
                CompareOperator.In,
                FieldComparisonValue.Array(PlanValues(values)),
                value => set.Contains(value));
        }

        public FieldGuard<T> NotIn(params TProp[] values)
        {
            var set = ValueSet(values);
            return _field.GuardWithOperand(
                CompareOperator.NotIn,
                FieldComparisonValue.Array(PlanValues(values)),
                value => !set.Contains(value));
        }

        public FieldGuard<T> Between(TProp low, TProp high)
        {
            var comparer = Comparer<TProp>.Default;
            return _field.GuardWithOperand(
                CompareOperator.Between,
                FieldComparisonValue.Array(new[] { ValidationConditionLiteral.From(low), ValidationConditionLiteral.From(high) }),
                value => comparer.Compare(value, low) >= 0 && comparer.Compare(value, high) <= 0);
        }

        public FieldGuard<T> ArrayContains(TProp value) =>
            _field.GuardAgainstCollectionItem(
                CompareOperator.ArrayContains,
                value,
                candidate => FieldConditionPredicates.ArrayContains(candidate, value));

        private FieldGuard<T> Unary(CompareOperator op, Func<TProp, bool> predicate) =>
            _field.Guard(op, predicate);

        private FieldGuard<T> Literal<TValue>(
            CompareOperator op,
            TValue value,
            Func<TProp, bool> predicate) =>
            _field.GuardAgainstLiteral(op, value, predicate);

        private FieldGuard<T> Compare(
            CompareOperator op,
            TProp value,
            Func<int, bool> accepts)
        {
            var comparer = Comparer<TProp>.Default;
            return Literal(op, value, candidate => accepts(comparer.Compare(candidate, value)));
        }

        private FieldGuard<T> Text(
            CompareOperator op,
            string operand,
            Func<string, bool> predicate) =>
            Literal(op, operand, value => value is string text && predicate(text));

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

        private static bool IsEmptyValue(TProp value)
        {
            if (value == null) return true;
            return value is string text && string.IsNullOrEmpty(text);
        }

        private static HashSet<TProp> ValueSet(TProp[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return new HashSet<TProp>(values);
        }

        private static IReadOnlyList<object?> PlanValues(TProp[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return values.Select(ValidationConditionLiteral.From).ToArray();
        }
    }

    public sealed class FieldGuard<T> where T : class
    {
        internal FieldCondition Condition { get; }
        internal IReadOnlyList<ClientValidationFieldReference> Fields { get; }
        internal Func<T, bool> ServerPredicate { get; }

        internal FieldGuard(
            FieldCondition condition,
            IEnumerable<ClientValidationFieldReference> fields,
            Func<T, bool> serverPredicate)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Fields = ClientValidationGuardFields.From(fields);
            ServerPredicate = serverPredicate ?? throw new ArgumentNullException(nameof(serverPredicate));
        }

        public FieldGuard<T> And(FieldGuard<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new FieldGuard<T>(
                FieldCondition.All(Condition, other.Condition),
                ClientValidationGuardFields.Combine(Fields, other.Fields),
                model => ServerPredicate(model) && other.ServerPredicate(model));
        }

        public FieldGuard<T> Or(FieldGuard<T> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return new FieldGuard<T>(
                FieldCondition.Any(Condition, other.Condition),
                ClientValidationGuardFields.Combine(Fields, other.Fields),
                model => ServerPredicate(model) || other.ServerPredicate(model));
        }

        public FieldGuard<T> Not() =>
            new FieldGuard<T>(FieldCondition.Not(Condition), Fields, model => !ServerPredicate(model));
    }

    internal static class ClientValidationGuardFields
    {
        internal static IReadOnlyList<ClientValidationFieldReference> From(IEnumerable<ClientValidationFieldReference> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            var byPath = new Dictionary<string, ClientValidationFieldReference>(StringComparer.Ordinal);

            foreach (var field in fields)
            {
                if (field == null) throw new ArgumentException("Client validation guard field must not be null.", nameof(fields));

                if (byPath.TryGetValue(field.Path.Value, out var existing))
                {
                    if (!existing.Shape.Equals(field.Shape))
                    {
                        throw new InvalidOperationException(
                            $"Client validation condition field '{field.Path.Value}' was declared with conflicting shapes: " +
                            $"'{existing.Shape.Kind}' and '{field.Shape.Kind}'.");
                    }

                    continue;
                }

                byPath.Add(field.Path.Value, field);
            }

            return byPath.Values.ToArray();
        }

        internal static IReadOnlyList<ClientValidationFieldReference> Combine(
            IEnumerable<ClientValidationFieldReference> first,
            IEnumerable<ClientValidationFieldReference> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            return From(first.Concat(second));
        }
    }

    internal static class FieldConditionPredicates
    {
        internal static bool EnumerableContains<TItem>(IEnumerable<TItem>? values, TItem expected) =>
            values != null && values.Contains(expected);

        internal static bool ArrayContains<TItem>(object? value, TItem expected)
        {
            if (!(value is IEnumerable enumerable)) return false;

            foreach (var item in enumerable)
            {
                if (Equals(item, expected)) return true;
            }

            return false;
        }
    }
}
