using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    internal sealed class ClientValidationLiteral
    {
        private ClientValidationLiteral(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal object? Value { get; }
        internal Shape Shape { get; }

        internal static ClientValidationLiteral From<TValue>(TValue value)
        {
            if (value == null)
                return new ClientValidationLiteral(null, Shape.None);

            var shape = Shape.FromClrType(value.GetType());
            return new ClientValidationLiteral(ValidationDateLiteral.From(value, shape), shape);
        }
    }
}
