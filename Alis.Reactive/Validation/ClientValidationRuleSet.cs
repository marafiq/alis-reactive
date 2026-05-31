using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    internal sealed class ClientValidationRuleSet
    {
        private readonly Dictionary<string, ClientValidationRuleSetField> _fields =
            new Dictionary<string, ClientValidationRuleSetField>(StringComparer.Ordinal);

        internal void EnsureFields(IEnumerable<ClientValidationFieldReference> fields)
        {
            foreach (var field in fields)
                EnsureField(field);
        }

        internal void EnsureField(ClientValidationFieldReference field)
        {
            if (_fields.TryGetValue(field.Path.Value, out var existing))
            {
                existing.AssertSameShape(field);
                return;
            }

            _fields.Add(field.Path.Value, new ClientValidationRuleSetField(field));
        }

        internal void AddRule(ClientValidationFieldReference field, ClientRule rule)
        {
            EnsureFields(rule.PeerFieldReferences);
            EnsureField(field);
            _fields[field.Path.Value].AddRule(rule);
        }

        internal void AddRule(
            ClientValidationFieldReference field,
            RuleName name,
            ValidationMessage message,
            RuleOperand operand,
            ClientRuleActivation activation,
            Shape shape) =>
            AddRule(field, new ClientRule(name, message, operand, activation, shape));

        internal void AddItemFields(
            ClientValidationFieldReference collection,
            IEnumerable<ClientValidationField> itemFields,
            ClientRuleActivation activation)
        {
            EnsureField(collection);
            foreach (var itemField in itemFields)
                _fields[collection.Path.Value].AddItemField(
                    itemField.PrefixedBy(ValidationFieldPath.Empty, activation));
        }

        internal void AddRulesFrom(
            IEnumerable<ClientValidationField> fields,
            ValidationFieldPath prefix,
            ClientRuleActivation activation)
        {
            foreach (var field in fields)
            {
                var target = field.PrefixedBy(prefix, activation);
                EnsureField(target.Reference);
                foreach (var rule in target.Rules)
                    AddRule(target.Reference, rule);
                foreach (var itemField in target.ItemFields)
                    _fields[target.FieldName].AddItemField(itemField);
            }
        }

        internal IReadOnlyList<ClientValidationField> ToFields() =>
            _fields.Values.Select(field => field.ToField()).ToArray();
    }

    internal sealed class ClientValidationRuleSetField
    {
        private readonly ClientValidationFieldReference _field;
        private readonly List<ClientRule> _rules = new List<ClientRule>();
        private readonly List<ClientValidationField> _itemFields = new List<ClientValidationField>();

        internal ClientValidationRuleSetField(ClientValidationFieldReference field)
        {
            _field = field;
        }

        internal void AssertSameShape(ClientValidationFieldReference field)
        {
            if (!_field.Shape.Equals(field.Shape))
            {
                throw new InvalidOperationException(
                    $"Client validation field '{_field.Path.Value}' was declared with conflicting shapes: " +
                    $"'{_field.Shape.Kind}' and '{field.Shape.Kind}'.");
            }
        }

        internal void AddRule(ClientRule rule)
        {
            _rules.Add(rule);
        }

        internal void AddItemField(ClientValidationField field)
        {
            _itemFields.Add(field);
        }

        internal ClientValidationField ToField() =>
            new ClientValidationField(
                _field,
                _rules,
                _itemFields);
    }
}
