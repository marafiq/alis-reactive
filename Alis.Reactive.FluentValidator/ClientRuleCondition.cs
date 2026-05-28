using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Alis.Reactive.Validation;

namespace Alis.Reactive.FluentValidator
{
    internal abstract class ClientRuleCondition
    {
        private protected ClientRuleCondition() { }

        internal static ClientRuleCondition FromGuard<TModel>(FieldGuard<TModel> guard)
            where TModel : class
        {
            if (guard == null) throw new ArgumentNullException(nameof(guard));
            return new BrowserClientRuleCondition(guard.Condition, guard.Fields);
        }

        internal static ClientRuleCondition All(IEnumerable<ClientRuleCondition> conditions)
        {
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));

            var conditionsForClient = new List<FieldCondition>();
            var fieldsReadByConditions = new List<ClientValidationFieldReference>();

            foreach (var condition in conditions)
            {
                if (condition == null)
                    throw new ArgumentException("Client rule condition must not be null.", nameof(conditions));

                if (!condition.TryUseOnClient(out var fieldCondition, out var fields))
                    return ServerOnly();

                conditionsForClient.Add(fieldCondition);
                fieldsReadByConditions.AddRange(fields);
            }

            return new BrowserClientRuleCondition(
                FieldCondition.All(conditionsForClient.ToArray()),
                ClientValidationGuardFields.From(fieldsReadByConditions));
        }

        internal static ClientRuleCondition ServerOnly() =>
            ServerOnlyClientCondition.Instance;

        internal abstract bool TryUseOnClient(
            [NotNullWhen(true)] out FieldCondition? condition,
            out IReadOnlyList<ClientValidationFieldReference> fields);

        private sealed class BrowserClientRuleCondition : ClientRuleCondition
        {
            private readonly FieldCondition _condition;
            private readonly IReadOnlyList<ClientValidationFieldReference> _fields;

            internal BrowserClientRuleCondition(
                FieldCondition condition,
                IReadOnlyList<ClientValidationFieldReference> fields)
            {
                _condition = condition ?? throw new ArgumentNullException(nameof(condition));
                _fields = ClientValidationGuardFields.From(fields);
            }

            internal override bool TryUseOnClient(
                [NotNullWhen(true)] out FieldCondition? condition,
                out IReadOnlyList<ClientValidationFieldReference> fields)
            {
                condition = _condition;
                fields = _fields;
                return true;
            }
        }

        private sealed class ServerOnlyClientCondition : ClientRuleCondition
        {
            internal static ServerOnlyClientCondition Instance { get; } = new ServerOnlyClientCondition();

            private ServerOnlyClientCondition() { }

            internal override bool TryUseOnClient(
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
