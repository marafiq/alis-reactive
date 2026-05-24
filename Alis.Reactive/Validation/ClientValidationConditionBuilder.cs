using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Builds typed field conditions for client validation projection.
    /// </summary>
    public sealed class ClientValidationConditionBuilder<TModel>
        where TModel : class
    {
        internal ClientValidationConditionBuilder() { }

        public ClientValidationFieldConditionStart<TModel, TValue> Field<TValue>(
            Expression<Func<TModel, TValue>> field) =>
            Field(ClientValidationFieldToken<TModel, TValue>.For(field));

        public ClientValidationFieldConditionStart<TModel, TValue> Field<TValue>(
            ClientValidationFieldToken<TModel, TValue> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            return new ClientValidationFieldConditionStart<TModel, TValue>(field);
        }
    }

    /// <summary>
    /// Starts a client validation condition from a typed model field.
    /// </summary>
    public sealed class ClientValidationFieldConditionStart<TModel, TValue>
        where TModel : class
    {
        private readonly ClientValidationFieldToken<TModel, TValue> _field;

        internal ClientValidationFieldConditionStart(ClientValidationFieldToken<TModel, TValue> field)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
        }

        public ClientValidationCondition<TModel> Truthy() =>
            Compare(CompareOperator.Truthy);

        public ClientValidationCondition<TModel> Falsy() =>
            Compare(CompareOperator.Falsy);

        public ClientValidationCondition<TModel> Eq(TValue value) =>
            CompareLiteral(CompareOperator.Eq, value);

        public ClientValidationCondition<TModel> Neq(TValue value) =>
            CompareLiteral(CompareOperator.Neq, value);

        public ClientValidationCondition<TModel> Gt(TValue value) =>
            CompareLiteral(CompareOperator.Gt, value);

        public ClientValidationCondition<TModel> Gte(TValue value) =>
            CompareLiteral(CompareOperator.Gte, value);

        public ClientValidationCondition<TModel> Lt(TValue value) =>
            CompareLiteral(CompareOperator.Lt, value);

        public ClientValidationCondition<TModel> Lte(TValue value) =>
            CompareLiteral(CompareOperator.Lte, value);

        public ClientValidationCondition<TModel> IsNull() =>
            Compare(CompareOperator.IsNull);

        public ClientValidationCondition<TModel> NotNull() =>
            Compare(CompareOperator.NotNull);

        public ClientValidationCondition<TModel> IsEmpty() =>
            Compare(CompareOperator.IsEmpty);

        public ClientValidationCondition<TModel> NotEmpty() =>
            Compare(CompareOperator.NotEmpty);

        public ClientValidationCondition<TModel> In(params TValue[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return CompareArray(CompareOperator.In, values);
        }

        public ClientValidationCondition<TModel> NotIn(params TValue[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return CompareArray(CompareOperator.NotIn, values);
        }

        public ClientValidationCondition<TModel> Between(TValue lowerBound, TValue upperBound) =>
            CompareArray(CompareOperator.Between, new[] { lowerBound, upperBound });

        public ClientValidationCondition<TModel> Contains(string substring) =>
            CompareLiteral(CompareOperator.Contains, substring);

        public ClientValidationCondition<TModel> StartsWith(string prefix) =>
            CompareLiteral(CompareOperator.StartsWith, prefix);

        public ClientValidationCondition<TModel> EndsWith(string suffix) =>
            CompareLiteral(CompareOperator.EndsWith, suffix);

        public ClientValidationCondition<TModel> Matches(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                throw new ArgumentException("A regex pattern is required for a client validation condition.", nameof(pattern));

            return CompareLiteral(CompareOperator.Matches, pattern);
        }

        public ClientValidationCondition<TModel> MinLength(int minLength)
        {
            var minimumLength = MinimumTextLength.From(minLength, nameof(minLength));
            return CompareLiteral(CompareOperator.MinLength, minimumLength.Value);
        }

        public ClientValidationCondition<TModel> ArrayContains<TItem>(TItem item)
        {
            var literal = ClientValidationProjectionLiteral.From(item);
            return Build(FieldCondition.Compare(
                _field.Reference.Path,
                CompareOperator.ArrayContains,
                FieldComparisonValue.CollectionItem(literal.Value, literal.Shape)));
        }

        private ClientValidationCondition<TModel> Compare(CompareOperator op)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            return Build(FieldCondition.Compare(_field.Reference.Path, op));
        }

        private ClientValidationCondition<TModel> CompareLiteral<TLiteral>(
            CompareOperator op,
            TLiteral value)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));

            var literal = ClientValidationProjectionLiteral.From(value);
            return Build(FieldCondition.Compare(_field.Reference.Path, op, literal.Value));
        }

        private ClientValidationCondition<TModel> CompareArray(
            CompareOperator op,
            IEnumerable<TValue> values)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (values == null) throw new ArgumentNullException(nameof(values));

            var literals = values
                .Select(value => ClientValidationProjectionLiteral.From(value).Value)
                .ToArray();
            return Build(FieldCondition.Compare(
                _field.Reference.Path,
                op,
                FieldComparisonValue.Array(literals)));
        }

        private ClientValidationCondition<TModel> Build(FieldCondition condition) =>
            ClientValidationCondition<TModel>.From(condition, new[] { _field.Reference });
    }

    /// <summary>
    /// Completed client validation condition that can be composed.
    /// </summary>
    public sealed class ClientValidationCondition<TModel>
        where TModel : class
    {
        private readonly IReadOnlyList<ClientValidationFieldReference> _fields;

        private ClientValidationCondition(
            FieldCondition condition,
            IReadOnlyList<ClientValidationFieldReference> fields)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
        }

        internal FieldCondition Condition { get; }
        internal IReadOnlyList<ClientValidationFieldReference> Fields => _fields;

        public ClientValidationCondition<TModel> And(ClientValidationCondition<TModel> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return From(
                FieldCondition.All(Condition, other.Condition),
                _fields.Concat(other.Fields));
        }

        public ClientValidationCondition<TModel> Or(ClientValidationCondition<TModel> other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            return From(
                FieldCondition.Any(Condition, other.Condition),
                _fields.Concat(other.Fields));
        }

        public ClientValidationCondition<TModel> Not() =>
            From(FieldCondition.Not(Condition), _fields);

        internal static ClientValidationCondition<TModel> From(
            FieldCondition condition,
            IEnumerable<ClientValidationFieldReference> fields)
        {
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            return new ClientValidationCondition<TModel>(
                condition,
                fields.ToArray());
        }
    }
}
