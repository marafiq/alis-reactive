using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    internal abstract class ClientConditionProjection
    {
        private protected ClientConditionProjection() { }

        internal static ClientConditionProjection Project<TModel>(FieldGuard<TModel> guard)
            where TModel : class
        {
            if (guard == null) throw new ArgumentNullException(nameof(guard));
            return new ProjectedClientCondition(guard.Condition, guard.Fields);
        }

        internal static ClientConditionProjection All(IEnumerable<ClientConditionProjection> conditions)
        {
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));

            var projectedConditions = new List<FieldCondition>();
            var projectedFields = new List<ClientValidationFieldReference>();

            foreach (var condition in conditions)
            {
                if (condition == null)
                    throw new ArgumentException("Client condition projection must not be null.", nameof(conditions));

                if (!condition.TryProject(out var fieldCondition, out var fields))
                    return Unprojected();

                projectedConditions.Add(fieldCondition);
                projectedFields.AddRange(fields);
            }

            return new ProjectedClientCondition(
                FieldCondition.All(projectedConditions.ToArray()),
                ClientValidationGuardFields.From(projectedFields));
        }

        internal static ClientConditionProjection Unprojected() =>
            UnprojectedClientCondition.Instance;

        internal abstract bool TryProject(
            [NotNullWhen(true)] out FieldCondition? condition,
            out IReadOnlyList<ClientValidationFieldReference> fields);

        private sealed class ProjectedClientCondition : ClientConditionProjection
        {
            private readonly FieldCondition _condition;
            private readonly IReadOnlyList<ClientValidationFieldReference> _fields;

            internal ProjectedClientCondition(
                FieldCondition condition,
                IReadOnlyList<ClientValidationFieldReference> fields)
            {
                _condition = condition ?? throw new ArgumentNullException(nameof(condition));
                _fields = ClientValidationGuardFields.From(fields);
            }

            internal override bool TryProject(
                [NotNullWhen(true)] out FieldCondition? condition,
                out IReadOnlyList<ClientValidationFieldReference> fields)
            {
                condition = _condition;
                fields = _fields;
                return true;
            }
        }

        private sealed class UnprojectedClientCondition : ClientConditionProjection
        {
            internal static UnprojectedClientCondition Instance { get; } = new UnprojectedClientCondition();

            private UnprojectedClientCondition() { }

            internal override bool TryProject(
                [NotNullWhen(true)] out FieldCondition? condition,
                out IReadOnlyList<ClientValidationFieldReference> fields)
            {
                condition = null;
                fields = Array.Empty<ClientValidationFieldReference>();
                return false;
            }
        }
    }

}
