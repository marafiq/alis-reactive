using System;
using System.Collections.Generic;
using System.Linq;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// Rule source declares the field path and shape before render-time binding.
    /// </summary>
    public sealed class ClientValidationField
    {
        private readonly ClientValidationFieldReference _field;

        // For a collection item field: the condition declared around ClientRuleEach, relative to the
        // collection's owner. Relocating the owner prefixes it; the rendered item prefix never does.
        private readonly ClientRuleActivation _ownerActivation;

        public string FieldName => _field.Path.Value;
        internal IReadOnlyList<ClientRule> Rules { get; }
        internal IReadOnlyList<ClientValidationField> ItemFields { get; }

        internal ClientValidationField(
            ClientValidationFieldReference field,
            IEnumerable<ClientRule> rules,
            IEnumerable<ClientValidationField> itemFields)
            : this(field, rules, itemFields, ClientRuleActivation.Always)
        {
        }

        private ClientValidationField(
            ClientValidationFieldReference field,
            IEnumerable<ClientRule> rules,
            IEnumerable<ClientValidationField> itemFields,
            ClientRuleActivation ownerActivation)
        {
            _field = field;
            Rules = rules.ToArray();
            ItemFields = itemFields.ToArray();
            _ownerActivation = ownerActivation;
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
                ItemFields.Select(field => field.OwnedUnder(prefix, activation)));
        }

        // A collection item field rendered at itemPrefix: its own conditions take the item prefix,
        // its owner's condition does not.
        internal ClientValidationField RenderedAt(ValidationFieldPath itemPrefix) =>
            PrefixedBy(itemPrefix, _ownerActivation);

        // An item field whose owner declared it under a condition (WhenField around ClientRuleEach).
        internal ClientValidationField ScopedBy(ClientRuleActivation activation) =>
            new ClientValidationField(_field, Rules, ItemFields, activation.Combine(_ownerActivation));

        // An item field whose collection's owner moves under prefix: its owner condition moves with it.
        private ClientValidationField OwnedUnder(ValidationFieldPath prefix, ClientRuleActivation activation) =>
            new ClientValidationField(_field, Rules, ItemFields, activation.Combine(_ownerActivation.PrefixedBy(prefix)));
    }
}
