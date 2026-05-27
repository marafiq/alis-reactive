using System;
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
}
