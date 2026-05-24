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
            Type modelType) =>
            new ValidationProjectionBindingScope(new ValidationFieldBindingCatalog(registeredInputs, modelType));

        internal BoundClientValidationField Bind(ClientValidationField field)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            return BoundClientValidationField.From(
                field,
                _fieldBindings.Resolve(field.FieldPath),
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
        private readonly Type _modelType;

        internal ValidationFieldBindingCatalog(
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType)
        {
            _registeredInputs = registeredInputs ?? throw new ArgumentNullException(nameof(registeredInputs));
            _modelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
        }

        internal ValidationFieldBinding Resolve(ValidationFieldPath fieldPath)
        {
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));

            if (_registeredInputs.TryGetValue(fieldPath.Value, out var registration))
                return ValidationFieldBinding.Registered(registration);

            var deferredField = DeferredModelBoundClientValidationField.For(_modelType, fieldPath);
            return ValidationFieldBinding.Deferred(deferredField);
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

        internal static DeferredModelBoundClientValidationField For(Type modelType, ValidationFieldPath fieldPath)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));

            var modelField = ValidationModelField.Resolve(modelType, fieldPath);
            return new DeferredModelBoundClientValidationField(
                Alis.Reactive.PlanModel.ComponentId.Of(IdGenerator.For(modelType, fieldPath.Value)),
                modelField.Shape);
        }
    }

    internal sealed class ValidationModelField
    {
        private ValidationModelField(Shape shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal Shape Shape { get; }

        internal static ValidationModelField Resolve(Type modelType, ValidationFieldPath fieldPath)
        {
            if (modelType == null) throw new ArgumentNullException(nameof(modelType));
            if (fieldPath == null) throw new ArgumentNullException(nameof(fieldPath));

            var currentType = modelType;
            foreach (var segment in fieldPath.Segments)
            {
                var member = ValidationModelMember.Find(modelType, currentType, segment, fieldPath);
                currentType = member.ValueType;
            }

            return new ValidationModelField(Shape.FromClrType(currentType));
        }
    }

    internal sealed class ValidationModelMember
    {
        private ValidationModelMember(Type valueType)
        {
            ValueType = valueType ?? throw new ArgumentNullException(nameof(valueType));
        }

        internal Type ValueType { get; }

        internal static ValidationModelMember Find(
            Type rootModelType,
            Type declaringType,
            string segment,
            ValidationFieldPath fullPath)
        {
            if (rootModelType == null) throw new ArgumentNullException(nameof(rootModelType));
            if (declaringType == null) throw new ArgumentNullException(nameof(declaringType));
            if (segment == null) throw new ArgumentNullException(nameof(segment));
            if (fullPath == null) throw new ArgumentNullException(nameof(fullPath));

            var property = declaringType.GetProperty(segment);
            if (property != null)
                return new ValidationModelMember(property.PropertyType);

            var field = declaringType.GetField(segment);
            if (field != null)
                return new ValidationModelMember(field.FieldType);

            throw new InvalidOperationException(
                $"Validation field '{fullPath.Value}' was projected for model '{rootModelType.FullName}', " +
                $"but segment '{segment}' is not a public property or field on '{declaringType.FullName}'. " +
                "Ensure the validator targets the model field rendered by the form, or register the input component for that binding path.");
        }
    }
}
