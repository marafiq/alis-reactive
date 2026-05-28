using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// The rule source declares the field path and shape before render-time binding.
    /// </summary>
    public sealed class ClientValidationField
    {
        private readonly ClientValidationFieldReference _field;

        public string FieldName => _field.Path.Value;
        public IReadOnlyList<ValidationRule> Rules { get; }
        internal IReadOnlyList<ClientValidationField> ItemFields { get; }

        internal ClientValidationField(
            ClientValidationFieldReference field,
            IEnumerable<ValidationRule> rules,
            IEnumerable<ClientValidationField>? itemFields = null)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            Rules = SnapshotRules(rules);
            ItemFields = SnapshotItemFields(itemFields ?? Array.Empty<ClientValidationField>());
        }

        internal ClientValidationFieldReference Reference => _field;

        internal ValidationFieldPath FieldPath => _field.Path;

        internal DeferredModelBoundClientValidationField ToDeferredField(Type modelType) =>
            DeferredModelBoundClientValidationField.ForClientRuleField(modelType, _field.Path, _field.Shape);

        internal bool HasRules => Rules.Count > 0;

        internal ClientValidationField PrefixedBy(ValidationFieldPath prefix, ClientRuleActivation activation)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            if (activation == null) throw new ArgumentNullException(nameof(activation));

            return new ClientValidationField(
                _field.PrefixedBy(prefix),
                Rules.Select(rule => rule.PrefixedBy(prefix, activation)),
                ItemFields.Select(field => field.ActivatedBy(activation)));
        }

        private ClientValidationField ActivatedBy(ClientRuleActivation activation)
        {
            if (activation == null) throw new ArgumentNullException(nameof(activation));

            return new ClientValidationField(
                _field,
                Rules.Select(rule => rule.PrefixedBy(ValidationFieldPath.Empty, activation)),
                ItemFields.Select(field => field.ActivatedBy(activation)));
        }

        private static IReadOnlyList<ValidationRule> SnapshotRules(IEnumerable<ValidationRule> rules)
        {
            if (rules == null) throw new ArgumentNullException(nameof(rules));

            var snapshot = new List<ValidationRule>();
            foreach (var rule in rules)
            {
                if (rule == null)
                    throw new ArgumentException("Validation rule must not be null.", nameof(rules));

                snapshot.Add(rule);
            }

            return snapshot.AsReadOnly();
        }

        private static IReadOnlyList<ClientValidationField> SnapshotItemFields(IEnumerable<ClientValidationField> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var snapshot = new List<ClientValidationField>();
            foreach (var field in fields)
            {
                if (field == null)
                    throw new ArgumentException("Client validation item field must not be null.", nameof(fields));

                snapshot.Add(field);
            }

            return snapshot.AsReadOnly();
        }
    }
}
