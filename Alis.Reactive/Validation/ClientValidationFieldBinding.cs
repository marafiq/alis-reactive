using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;
using Alis.Reactive.Validation;

namespace Alis.Reactive
{
    internal sealed class ClientValidationFieldBinder
    {
        private readonly IReadOnlyDictionary<string, ComponentRegistration> _registeredInputs;
        private readonly IReadOnlyDictionary<string, ClientValidationField> _clientRuleFields;
        private readonly Type _modelType;

        internal ClientValidationFieldBinder(
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType,
            IReadOnlyList<ClientValidationField> clientRuleFields)
        {
            _registeredInputs = registeredInputs;
            _modelType = modelType;
            _clientRuleFields = IndexClientRuleFields(clientRuleFields);
        }

        internal ValidationFieldBinding Resolve(ClientValidationField field)
        {
            if (_registeredInputs.TryGetValue(field.FieldName, out var registration))
                return ValidationFieldBinding.Registered(registration);

            return ValidationFieldBinding.Deferred(field.ToDeferredField(_modelType));
        }

        internal IEnumerable<ComponentValidation> ResolveAll(
            ClientValidationField field,
            ValidationPlanBinding ruleBinding)
        {
            if (field.HasRules)
                yield return Resolve(field).ToComponentValidation(field, ruleBinding);

            foreach (var itemField in ExpandRenderedItemFields(field))
                yield return Resolve(itemField).ToComponentValidation(itemField, ruleBinding);
        }

        internal ValidationFieldBinding Resolve(ValidationFieldPath fieldPath)
        {
            if (_registeredInputs.TryGetValue(fieldPath.Value, out var registration))
                return ValidationFieldBinding.Registered(registration);

            if (_clientRuleFields.TryGetValue(fieldPath.Value, out var field))
                return ValidationFieldBinding.Deferred(field.ToDeferredField(_modelType));

            throw new InvalidOperationException(
                $"Validation field '{fieldPath.Value}' was referenced by a client validation rule for model '{_modelType.FullName}', " +
                "but that field was not included in the client validation rule set. " +
                "Declare peer fields and condition fields through the same typed client rules so their shape is known before render-time binding.");
        }

        private IEnumerable<ClientValidationField> ExpandRenderedItemFields(ClientValidationField collectionField)
        {
            if (collectionField.ItemFields.Count == 0) yield break;

            foreach (var registeredPath in _registeredInputs.Keys)
                foreach (var itemField in ExpandRenderedItemFields(
                             collectionField.FieldName,
                             collectionField.ItemFields,
                             registeredPath))
                    yield return itemField;
        }

        private static IEnumerable<ClientValidationField> ExpandRenderedItemFields(
            string collectionPath,
            IReadOnlyList<ClientValidationField> itemFields,
            string registeredPath)
        {
            foreach (var itemField in itemFields)
            {
                if (!TryMatchCollectionItemMember(
                        collectionPath,
                        itemField.FieldName,
                        registeredPath,
                        out var itemPrefix,
                        out var directMatch))
                    continue;

                var renderedField = itemField.PrefixedBy(
                    ValidationFieldPath.Of(itemPrefix),
                    ClientRuleActivation.Always);

                if (directMatch && renderedField.HasRules)
                    yield return renderedField;

                foreach (var nestedItemField in ExpandRenderedItemFields(
                             renderedField.FieldName,
                             renderedField.ItemFields,
                             registeredPath))
                    yield return nestedItemField;
            }
        }

        private static bool TryMatchCollectionItemMember(
            string collectionPath,
            string itemFieldPath,
            string registeredPath,
            out string itemPrefix,
            out bool directMatch)
        {
            itemPrefix = string.Empty;
            directMatch = false;

            var openingBracket = collectionPath.Length;
            if (!registeredPath.StartsWith(collectionPath + "[", StringComparison.Ordinal))
                return false;

            var closingBracket = registeredPath.IndexOf(']', openingBracket + 1);
            if (closingBracket < 0)
                return false;

            itemPrefix = registeredPath.Substring(0, closingBracket + 1);

            var renderedItemFieldPath = itemPrefix + "." + itemFieldPath;
            directMatch = string.Equals(registeredPath, renderedItemFieldPath, StringComparison.Ordinal);
            if (directMatch)
                return true;

            if (!registeredPath.StartsWith(renderedItemFieldPath + ".", StringComparison.Ordinal) &&
                !registeredPath.StartsWith(renderedItemFieldPath + "[", StringComparison.Ordinal))
                return false;

            return true;
        }

        private static IReadOnlyDictionary<string, ClientValidationField> IndexClientRuleFields(
            IReadOnlyList<ClientValidationField> fields)
        {
            var fieldsByPath = new Dictionary<string, ClientValidationField>(StringComparer.Ordinal);
            foreach (var field in fields)
                fieldsByPath[field.FieldName] = field;

            return fieldsByPath;
        }
    }

    internal sealed class ValidationFieldBinding
    {
        private readonly ComponentId _componentId;
        private readonly InputValueContract _valueContract;

        private ValidationFieldBinding(ComponentId componentId, InputValueContract valueContract)
        {
            _componentId = componentId;
            _valueContract = valueContract;
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
            return new ValidationFieldBinding(
                Alis.Reactive.PlanModel.ComponentId.Of(registration.ComponentId),
                registration.ValueContract);
        }

        internal static ValidationFieldBinding Deferred(DeferredModelBoundClientValidationField field)
        {
            return new ValidationFieldBinding(field.ComponentId, field.ValueContract);
        }
    }

    internal sealed class DeferredModelBoundClientValidationField
    {
        private DeferredModelBoundClientValidationField(ComponentId componentId, Shape shape)
        {
            ComponentId = componentId;
            Shape = shape;
        }

        internal ComponentId ComponentId { get; }
        internal Shape Shape { get; }

        internal InputValueContract ValueContract => InputValueContract.ForCanonicalValue(Shape);

        internal static DeferredModelBoundClientValidationField ForClientRuleField(
            Type modelType,
            ValidationFieldPath fieldPath,
            Shape shape)
        {
            return new DeferredModelBoundClientValidationField(
                Alis.Reactive.PlanModel.ComponentId.Of(IdGenerator.For(modelType, fieldPath.Value)),
                shape);
        }
    }
}
