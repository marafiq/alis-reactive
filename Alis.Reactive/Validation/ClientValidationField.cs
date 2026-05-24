using System;
using System.Collections.Generic;
using Alis.Reactive;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// Describes a single field's validation rules within a form.
    /// Enriched at render time from registered input component contracts.
    /// </summary>
    public sealed class ClientValidationField
    {
        private readonly ValidationFieldPath _fieldName;
        private readonly ClientValidationFieldShapeSource _shapeSource;

        public string FieldName => _fieldName.Value;
        public List<ValidationRule> Rules { get; }

        internal ClientValidationField(ValidationFieldPath fieldName, List<ValidationRule> rules)
            : this(fieldName, ClientValidationFieldShapeSource.ModelField, rules)
        {
        }

        internal ClientValidationField(
            ValidationFieldPath fieldName,
            ClientValidationFieldShapeSource shapeSource,
            List<ValidationRule> rules)
        {
            _fieldName = fieldName ?? throw new ArgumentNullException(nameof(fieldName));
            _shapeSource = shapeSource ?? throw new ArgumentNullException(nameof(shapeSource));
            Rules = ProjectedValidationRules.From(rules).ToPublicList();
        }

        internal ValidationFieldPath FieldPath => _fieldName;

        internal DeferredModelBoundClientValidationField ToDeferredField(Type modelType) =>
            _shapeSource.ToDeferredField(modelType, _fieldName);

        internal ClientValidationField Snapshot() =>
            new ClientValidationField(_fieldName, _shapeSource, Rules);
    }

    internal abstract class ClientValidationFieldShapeSource
    {
        private protected ClientValidationFieldShapeSource() { }

        internal static ClientValidationFieldShapeSource ModelField { get; } =
            new ModelFieldShapeSource();

        internal static ClientValidationFieldShapeSource Projected(Shape shape)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            return new ProjectedFieldShapeSource(shape);
        }

        internal abstract DeferredModelBoundClientValidationField ToDeferredField(
            Type modelType,
            ValidationFieldPath fieldPath);

        private sealed class ModelFieldShapeSource : ClientValidationFieldShapeSource
        {
            internal override DeferredModelBoundClientValidationField ToDeferredField(
                Type modelType,
                ValidationFieldPath fieldPath) =>
                DeferredModelBoundClientValidationField.FromModel(modelType, fieldPath);
        }

        private sealed class ProjectedFieldShapeSource : ClientValidationFieldShapeSource
        {
            private readonly Shape _shape;

            internal ProjectedFieldShapeSource(Shape shape)
            {
                _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            }

            internal override DeferredModelBoundClientValidationField ToDeferredField(
                Type modelType,
                ValidationFieldPath fieldPath) =>
                DeferredModelBoundClientValidationField.FromProjectedShape(modelType, fieldPath, _shape);
        }
    }
}
