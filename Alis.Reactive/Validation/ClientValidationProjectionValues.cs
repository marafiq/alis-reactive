using System;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    internal sealed class ClientValidationProjectionLiteral
    {
        private ClientValidationProjectionLiteral(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal object? Value { get; }
        internal Shape Shape { get; }

        internal static ClientValidationProjectionLiteral From<TValue>(TValue value)
        {
            if (value == null)
                return new ClientValidationProjectionLiteral(null, Shape.None);

            var shape = Shape.FromClrType(value.GetType());
            return new ClientValidationProjectionLiteral(ValidationDateLiteral.From(value, shape), shape);
        }
    }

    internal sealed class ClientValidationProjectionRangeBounds
    {
        private ClientValidationProjectionRangeBounds(object lowerBound, object upperBound, Shape endpointShape)
        {
            LowerBound = lowerBound ?? throw new ArgumentNullException(nameof(lowerBound));
            UpperBound = upperBound ?? throw new ArgumentNullException(nameof(upperBound));
            EndpointShape = endpointShape ?? throw new ArgumentNullException(nameof(endpointShape));
        }

        internal object LowerBound { get; }
        internal object UpperBound { get; }
        internal Shape EndpointShape { get; }

        internal static ClientValidationProjectionRangeBounds From<TValue>(TValue lowerBound, TValue upperBound)
        {
            if (lowerBound == null) throw new ArgumentNullException(nameof(lowerBound));
            if (upperBound == null) throw new ArgumentNullException(nameof(upperBound));
            if (TryFrom(lowerBound, upperBound, out var bounds)) return bounds;

            var lowerShape = ClientValidationProjectionLiteral.From(lowerBound).Shape;
            var upperShape = ClientValidationProjectionLiteral.From(upperBound).Shape;
            throw new ArgumentException(
                "Client validation range bounds must have the same shape. " +
                $"Lower bound is '{lowerShape.Kind}', upper bound is '{upperShape.Kind}'.");
        }

        internal static bool TryFrom(
            object? lowerBound,
            object? upperBound,
            [NotNullWhen(true)] out ClientValidationProjectionRangeBounds? bounds)
        {
            bounds = null;
            if (lowerBound == null || upperBound == null) return false;
            var lowerLiteral = ClientValidationProjectionLiteral.From(lowerBound);
            var upperLiteral = ClientValidationProjectionLiteral.From(upperBound);
            if (!lowerLiteral.Shape.Equals(upperLiteral.Shape)) return false;
            if (lowerLiteral.Value == null || upperLiteral.Value == null) return false;

            bounds = new ClientValidationProjectionRangeBounds(
                lowerLiteral.Value,
                upperLiteral.Value,
                lowerLiteral.Shape);
            return true;
        }

        internal ValidationRangeBounds ToValidationRangeBounds() =>
            ValidationRangeBounds.Between(LowerBound, UpperBound, EndpointShape);
    }
}
