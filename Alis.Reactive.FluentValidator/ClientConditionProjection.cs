using System;
using System.Collections.Generic;
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

                condition.Match(
                    (fieldCondition, fields) =>
                    {
                        projectedConditions.Add(fieldCondition);
                        projectedFields.AddRange(fields);
                        return true;
                    },
                    _ => true);
            }

            return new ProjectedClientCondition(
                FieldCondition.All(projectedConditions.ToArray()),
                ClientValidationGuardFields.From(projectedFields));
        }

        internal static ClientConditionProjection Skip(ClientRuleProjectionSkipReason reason) =>
            new SkippedClientCondition(reason);

        internal abstract TResult Match<TResult>(
            Func<FieldCondition, IReadOnlyList<ClientValidationFieldReference>, TResult> projected,
            Func<ClientRuleProjectionSkipReason, TResult> skipped);

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

            internal override TResult Match<TResult>(
                Func<FieldCondition, IReadOnlyList<ClientValidationFieldReference>, TResult> projected,
                Func<ClientRuleProjectionSkipReason, TResult> skipped)
            {
                if (projected == null) throw new ArgumentNullException(nameof(projected));
                if (skipped == null) throw new ArgumentNullException(nameof(skipped));
                return projected(_condition, _fields);
            }
        }

        private sealed class SkippedClientCondition : ClientConditionProjection
        {
            private readonly ClientRuleProjectionSkipReason _reason;

            internal SkippedClientCondition(ClientRuleProjectionSkipReason reason)
            {
                _reason = reason;
            }

            internal override TResult Match<TResult>(
                Func<FieldCondition, IReadOnlyList<ClientValidationFieldReference>, TResult> projected,
                Func<ClientRuleProjectionSkipReason, TResult> skipped)
            {
                if (projected == null) throw new ArgumentNullException(nameof(projected));
                if (skipped == null) throw new ArgumentNullException(nameof(skipped));
                return skipped(_reason);
            }
        }
    }

}
