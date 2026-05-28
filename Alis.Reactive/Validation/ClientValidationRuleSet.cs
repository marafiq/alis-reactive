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
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            foreach (var field in fields)
                EnsureField(field);
        }

        internal void EnsureField(ClientValidationFieldReference field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            if (_fields.TryGetValue(field.Path.Value, out var existing))
            {
                existing.AssertSameShape(field);
                return;
            }

            _fields.Add(field.Path.Value, new ClientValidationRuleSetField(field));
        }

        internal void AddRule(ClientValidationFieldReference field, ValidationRule rule)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            EnsureFields(rule.PeerFieldReferences);
            EnsureField(field);
            _fields[field.Path.Value].AddRule(rule);
        }

        internal void AddRule(
            ClientValidationFieldReference field,
            ValidationRuleName name,
            ValidationMessage message,
            ValidationRuleOperand operand,
            ClientRuleActivation activation,
            Shape shape) =>
            AddRule(field, new ValidationRule(name, message, operand, activation, shape));

        internal void AddItemFields(
            ClientValidationFieldReference collection,
            IEnumerable<ClientValidationField> itemFields,
            ClientRuleActivation activation)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));
            if (itemFields == null) throw new ArgumentNullException(nameof(itemFields));
            if (activation == null) throw new ArgumentNullException(nameof(activation));

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
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (activation == null) throw new ArgumentNullException(nameof(activation));

            foreach (var field in fields)
            {
                if (field == null)
                    throw new ArgumentException("Client validation field must not be null.", nameof(fields));

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
        private readonly List<ValidationRule> _rules = new List<ValidationRule>();
        private readonly List<ClientValidationField> _itemFields = new List<ClientValidationField>();

        internal ClientValidationRuleSetField(ClientValidationFieldReference field)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
        }

        internal void AssertSameShape(ClientValidationFieldReference field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            if (!_field.Shape.Equals(field.Shape))
            {
                throw new InvalidOperationException(
                    $"Client validation field '{_field.Path.Value}' was declared with conflicting shapes: " +
                    $"'{_field.Shape.Kind}' and '{field.Shape.Kind}'.");
            }
        }

        internal void AddRule(ValidationRule rule)
        {
            if (rule == null) throw new ArgumentNullException(nameof(rule));
            _rules.Add(rule);
        }

        internal void AddItemField(ClientValidationField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            _itemFields.Add(field);
        }

        internal ClientValidationField ToField() =>
            new ClientValidationField(
                _field,
                _rules,
                _itemFields);
    }
}
