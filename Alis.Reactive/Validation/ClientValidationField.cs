using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// Rule source declares the field path and shape before render-time binding.
    /// </summary>
    public sealed class ClientValidationField
    {
        private readonly ClientValidationFieldReference _field;

        public string FieldName => _field.Path.Value;
        internal IReadOnlyList<ClientRule> Rules { get; }
        internal IReadOnlyList<ClientValidationField> ItemFields { get; }

        internal ClientValidationField(
            ClientValidationFieldReference field,
            IEnumerable<ClientRule> rules,
            IEnumerable<ClientValidationField>? itemFields = null)
        {
            _field = field;
            Rules = rules.ToArray();
            ItemFields = (itemFields ?? Enumerable.Empty<ClientValidationField>()).ToArray();
        }

        internal ClientValidationFieldReference Reference => _field;

        internal ModelFieldInput ToModelFieldInput(Type modelType) =>
            ModelFieldInput.For(modelType, _field.Path, _field.Shape);

        internal bool HasRules => Rules.Count > 0;

        internal ClientValidationField PrefixedBy(ValidationFieldPath prefix, ClientRuleActivation activation)
        {
            return new ClientValidationField(
                _field.PrefixedBy(prefix),
                Rules.Select(rule => rule.PrefixedBy(prefix, activation)),
                ItemFields.Select(field => field.ActivatedBy(activation)));
        }

        private ClientValidationField ActivatedBy(ClientRuleActivation activation)
        {
            return new ClientValidationField(
                _field,
                Rules.Select(rule => rule.PrefixedBy(ValidationFieldPath.Empty, activation)),
                ItemFields.Select(field => field.ActivatedBy(activation)));
        }
    }
}
