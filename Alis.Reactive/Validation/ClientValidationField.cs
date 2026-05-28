using System;
using System.Collections.Generic;
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

        internal ClientValidationField(
            ClientValidationFieldReference field,
            IEnumerable<ValidationRule> rules)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            Rules = SnapshotRules(rules);
        }

        internal ClientValidationFieldReference Reference => _field;

        internal ValidationFieldPath FieldPath => _field.Path;

        internal DeferredModelBoundClientValidationField ToDeferredField(Type modelType) =>
            DeferredModelBoundClientValidationField.ForClientRuleField(modelType, _field.Path, _field.Shape);

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
    }
}
