using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.Validation
{
    internal sealed class ClientValidationProjectionDraft
    {
        private readonly Dictionary<string, ClientValidationProjectedField> _fields =
            new Dictionary<string, ClientValidationProjectedField>(StringComparer.Ordinal);

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

            _fields.Add(field.Path.Value, new ClientValidationProjectedField(field));
        }

        internal void AddRule(ClientValidationFieldReference field, ValidationRule rule)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (rule == null) throw new ArgumentNullException(nameof(rule));

            EnsureField(field);
            _fields[field.Path.Value].AddRule(rule);
        }

        internal IReadOnlyList<ClientValidationField> ToFields() =>
            _fields.Values.Select(field => field.ToField()).ToArray();
    }

    internal sealed class ClientValidationProjectedField
    {
        private readonly ClientValidationFieldReference _field;
        private readonly List<ValidationRule> _rules = new List<ValidationRule>();

        internal ClientValidationProjectedField(ClientValidationFieldReference field)
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

        internal ClientValidationField ToField() =>
            new ClientValidationField(
                _field,
                _rules);
    }
}
