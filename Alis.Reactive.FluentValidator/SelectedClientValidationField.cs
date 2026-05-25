using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    internal sealed class SelectedClientValidationField<TModel, TValue>
        where TModel : class
    {
        private readonly Func<TModel, TValue> _readValue;
        private readonly ClientValidationFieldReference _reference;

        private SelectedClientValidationField(
            ClientValidationFieldReference reference,
            Func<TModel, TValue> readValue)
        {
            _reference = reference ?? throw new ArgumentNullException(nameof(reference));
            _readValue = readValue ?? throw new ArgumentNullException(nameof(readValue));
        }

        private ValidationFieldPath Path => _reference.Path;

        internal ClientValidationFieldReference Reference => _reference;

        internal static SelectedClientValidationField<TModel, TValue> From(
            Expression<Func<TModel, TValue>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            return new SelectedClientValidationField<TModel, TValue>(
                FieldReferenceFrom(expression),
                expression.Compile());
        }

        internal FieldGuard<TModel> Guard(
            CompareOperator op,
            Func<TValue, bool> acceptsValue) =>
            GuardWithOperand(op, FieldComparisonValue.None, acceptsValue);

        internal FieldGuard<TModel> GuardAgainstLiteral(
            CompareOperator op,
            object? literal,
            Func<TValue, bool> acceptsValue) =>
            GuardWithOperand(
                op,
                FieldComparisonValue.Literal(ValidationConditionLiteral.From(literal)),
                acceptsValue);

        internal FieldGuard<TModel> GuardAgainstCollectionItem<TItem>(
            CompareOperator op,
            TItem item,
            Func<TValue, bool> acceptsValue) =>
            GuardWithOperand(
                op,
                FieldComparisonValue.CollectionItem(
                    ValidationConditionLiteral.From(item),
                    Shape.FromClrType(typeof(TItem))),
                acceptsValue);

        internal FieldGuard<TModel> GuardWithOperand(
            CompareOperator op,
            FieldComparisonValue operand,
            Func<TValue, bool> acceptsValue)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            if (operand == null) throw new ArgumentNullException(nameof(operand));
            if (acceptsValue == null) throw new ArgumentNullException(nameof(acceptsValue));

            bool FieldSatisfiesGuard(TModel model)
            {
                var value = _readValue(model);
                return acceptsValue(value);
            }

            return new FieldGuard<TModel>(
                FieldCondition.Compare(Path, op, operand),
                new[] { Reference },
                FieldSatisfiesGuard);
        }

        private static ClientValidationFieldReference FieldReferenceFrom(Expression<Func<TModel, TValue>> expression)
        {
            return ClientValidationFieldReference.Of(
                ValidationFieldPath.Of(ExpressionPathHelper.ToPropertyName(expression)),
                Shape.FromClrType(ExpressionPathHelper.ToPropertyType(expression)));
        }
    }

    internal static class ValidationConditionLiteral
    {
        internal static object? From<TValue>(TValue value)
        {
            if (value == null) return null;

            var shape = Shape.FromClrType(value.GetType());
            return ValidationDateLiteral.From(value, shape);
        }
    }
}
