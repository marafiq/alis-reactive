using System;
using System.Linq.Expressions;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Opaque typed reference to a model field used by client validation rules.
    /// </summary>
    public sealed class ClientValidationFieldToken<TModel, TValue>
        where TModel : class
    {
        private ClientValidationFieldToken(ClientValidationFieldReference reference)
        {
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        }

        internal ClientValidationFieldReference Reference { get; }

        public static ClientValidationFieldToken<TModel, TValue> For(
            Expression<Func<TModel, TValue>> field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            var path = ValidationFieldPath.Of(ExpressionPathHelper.ToPropertyName(field));
            var shape = Shape.FromClrType(ExpressionPathHelper.ToPropertyType(field));
            return new ClientValidationFieldToken<TModel, TValue>(
                ClientValidationFieldReference.Of(path, shape));
        }
    }

    internal sealed class ClientValidationFieldReference
    {
        private ClientValidationFieldReference(ValidationFieldPath path, Shape shape)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal ValidationFieldPath Path { get; }
        internal Shape Shape { get; }

        internal static ClientValidationFieldReference Of(ValidationFieldPath path, Shape shape) =>
            new ClientValidationFieldReference(path, shape);

        internal ClientValidationFieldReference PrefixedBy(ValidationFieldPath prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            return Of(prefix.Append(Path), Shape);
        }
    }
}
