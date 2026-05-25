using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    internal sealed class ValidationProjectionBindingScope
    {
        private readonly ValidationFieldBindingCatalog _fieldBindings;
        private readonly ValidationPlanBinding _ruleBinding;

        private ValidationProjectionBindingScope(ValidationFieldBindingCatalog fieldBindings)
        {
            _fieldBindings = fieldBindings ?? throw new ArgumentNullException(nameof(fieldBindings));
            _ruleBinding = ValidationPlanBinding.For(_fieldBindings);
        }

        internal static ValidationProjectionBindingScope For(
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType,
            IReadOnlyList<ClientValidationField> projectedFields) =>
            new ValidationProjectionBindingScope(new ValidationFieldBindingCatalog(registeredInputs, modelType, projectedFields));

        internal BoundClientValidationField Bind(ClientValidationField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            return BoundClientValidationField.From(
                field,
                _fieldBindings.Resolve(field),
                _ruleBinding);
        }
    }

    internal sealed class BoundClientValidationField
    {
        private readonly ClientValidationField _field;
        private readonly ValidationFieldBinding _binding;
        private readonly ValidationPlanBinding _ruleBinding;

        private BoundClientValidationField(
            ClientValidationField field,
            ValidationFieldBinding binding,
            ValidationPlanBinding ruleBinding)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
            _ruleBinding = ruleBinding ?? throw new ArgumentNullException(nameof(ruleBinding));
        }

        internal ComponentValidation ToComponentValidation()
        {
            var planRules = _field.Rules
                .Select(rule => rule.ToPlanRule(_ruleBinding))
                .ToList();

            return ComponentValidation.ForServerField(
                _binding.ComponentId,
                _binding.ReadValue(),
                planRules,
                _field.FieldName);
        }

        internal static BoundClientValidationField From(
            ClientValidationField field,
            ValidationFieldBinding binding,
            ValidationPlanBinding ruleBinding) =>
            new BoundClientValidationField(field, binding, ruleBinding);
    }

    internal sealed class ValidationFieldBindingCatalog
    {
        private readonly IReadOnlyDictionary<string, ComponentRegistration> _registeredInputs;
        private readonly IReadOnlyDictionary<string, ClientValidationField> _projectedFields;
        private readonly Type _modelType;

        internal ValidationFieldBindingCatalog(
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType,
            IReadOnlyList<ClientValidationField> projectedFields)
        {
            _registeredInputs = registeredInputs ?? throw new ArgumentNullException(nameof(registeredInputs));
            _modelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            _projectedFields = ProjectedValidationFieldCatalog.From(projectedFields);
        }

        internal ValidationFieldBinding Resolve(ClientValidationField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            if (_registeredInputs.TryGetValue(field.FieldName, out var registration))
                return ValidationFieldBinding.Registered(registration);

            return ValidationFieldBinding.Deferred(field.ToDeferredField(_modelType));
        }

        internal ValidationFieldBinding Resolve(ValidationFieldPath fieldPath)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));

            if (_registeredInputs.TryGetValue(fieldPath.Value, out var registration))
                return ValidationFieldBinding.Registered(registration);

            if (_projectedFields.TryGetValue(fieldPath.Value, out var field))
                return ValidationFieldBinding.Deferred(field.ToDeferredField(_modelType));

            throw new InvalidOperationException(
                $"Validation field '{fieldPath.Value}' was referenced by a projected validation rule for model '{_modelType.FullName}', " +
                "but that field was not included in the client validation projection. " +
                "Declare peer fields and condition fields through the same typed client projection so their shape is known before render-time binding.");
        }
    }

    internal static class ProjectedValidationFieldCatalog
    {
        internal static IReadOnlyDictionary<string, ClientValidationField> From(IReadOnlyList<ClientValidationField> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var catalog = new Dictionary<string, ClientValidationField>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                if (field == null)
                    throw new ArgumentException("Projected validation field must not be null.", nameof(fields));

                catalog[field.FieldName] = field;
            }

            return catalog;
        }
    }

    internal sealed class ValidationFieldBinding
    {
        private readonly ComponentId _componentId;
        private readonly InputValueContract _valueContract;

        private ValidationFieldBinding(ComponentId componentId, InputValueContract valueContract)
        {
            _componentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            _valueContract = valueContract ?? throw new ArgumentNullException(nameof(valueContract));
        }

        internal string ComponentId => _componentId.Value;

        internal Shape ShapeForValidation => _valueContract.Shape;

        internal ValueProducer ReadValue() =>
            ValueProducer.Read(
                ComponentSource.Of(ComponentId),
                _valueContract.ValueMember,
                shape: ShapeForValidation);

        internal FieldComparisonTarget ReadConditionTarget() =>
            FieldComparisonTarget.ForComponentValue(
                ValueProducer.Read(ComponentSource.Of(ComponentId), _valueContract.ValueMember),
                ShapeForValidation);

        internal static ValidationFieldBinding Registered(ComponentRegistration registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            return new ValidationFieldBinding(
                Alis.Reactive.PlanModel.ComponentId.Of(registration.ComponentId),
                registration.ValueContract);
        }

        internal static ValidationFieldBinding Deferred(DeferredModelBoundClientValidationField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));

            return new ValidationFieldBinding(field.ComponentId, field.ValueContract);
        }
    }

    internal sealed class DeferredModelBoundClientValidationField
    {
        private DeferredModelBoundClientValidationField(ComponentId componentId, Shape shape)
        {
            ComponentId = componentId ?? throw new ArgumentNullException(nameof(componentId));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal ComponentId ComponentId { get; }
        internal Shape Shape { get; }

        internal InputValueContract ValueContract => InputValueContract.ForCanonicalValue(Shape);

        internal static DeferredModelBoundClientValidationField ForProjectedField(
            Type modelType,
            ValidationFieldPath fieldPath,
            Shape shape)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));
            if (shape == null) throw new ArgumentNullException(nameof(shape));

            return new DeferredModelBoundClientValidationField(
                Alis.Reactive.PlanModel.ComponentId.Of(IdGenerator.For(modelType, fieldPath.Value)),
                shape);
        }
    }
}
