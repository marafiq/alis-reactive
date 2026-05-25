using System;
using System.Collections.Generic;
using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// The projection declares the field path and shape before render-time binding.
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
            Rules = ProjectedValidationRules.From(rules).ToReadOnlyList();
        }

        internal ValidationFieldPath FieldPath => _field.Path;

        internal DeferredModelBoundClientValidationField ToDeferredField(Type modelType) =>
            DeferredModelBoundClientValidationField.ForProjectedField(modelType, _field.Path, _field.Shape);

        internal ClientValidationField Snapshot() =>
            new ClientValidationField(_field, Rules);
    }
}
