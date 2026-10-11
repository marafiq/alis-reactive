using System;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
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
