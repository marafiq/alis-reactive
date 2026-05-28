using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    internal sealed class ValidationFieldBindingCatalog
    {
        private readonly IReadOnlyDictionary<string, ComponentRegistration> _registeredInputs;
        private readonly IReadOnlyDictionary<string, ClientValidationField> _clientRuleFields;
        private readonly Type _modelType;

        internal ValidationFieldBindingCatalog(
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType,
            IReadOnlyList<ClientValidationField> clientRuleFields)
        {
            _registeredInputs = registeredInputs ?? throw new ArgumentNullException(nameof(registeredInputs));
            _modelType = modelType ?? throw new ArgumentNullException(nameof(modelType));
            _clientRuleFields = IndexClientRuleFields(clientRuleFields);
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

            if (_clientRuleFields.TryGetValue(fieldPath.Value, out var field))
                return ValidationFieldBinding.Deferred(field.ToDeferredField(_modelType));

            throw new InvalidOperationException(
                $"Validation field '{fieldPath.Value}' was referenced by a client validation rule for model '{_modelType.FullName}', " +
                "but that field was not included in the client validation rule set. " +
                "Declare peer fields and condition fields through the same typed client rules so their shape is known before render-time binding.");
        }

        private static IReadOnlyDictionary<string, ClientValidationField> IndexClientRuleFields(
            IReadOnlyList<ClientValidationField> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var catalog = new Dictionary<string, ClientValidationField>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                if (field == null)
                    throw new ArgumentException("Client validation field must not be null.", nameof(fields));

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

        internal ValueExpression ReadValue() =>
            ValueExpression.Read(
                ComponentSource.Of(ComponentId),
                _valueContract.ValueMember,
                shape: ShapeForValidation);

        internal FieldComparisonTarget ReadConditionTarget() =>
            FieldComparisonTarget.ForComponentValue(
                ValueExpression.Read(ComponentSource.Of(ComponentId), _valueContract.ValueMember),
                ShapeForValidation);

        internal ComponentValidation ToComponentValidation(
            ClientValidationField field,
            ValidationPlanBinding ruleBinding)
        {
            if (field == null) throw new ArgumentNullException(nameof(field));
            if (ruleBinding == null) throw new ArgumentNullException(nameof(ruleBinding));

            var planRules = field.Rules
                .Select(rule => rule.ToPlanRule(ruleBinding))
                .ToList();

            return ComponentValidation.ForServerField(
                ComponentId,
                ReadValue(),
                planRules,
                field.FieldName);
        }

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

        internal static DeferredModelBoundClientValidationField ForClientRuleField(
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
