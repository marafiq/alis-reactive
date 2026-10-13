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

            return ValidationFieldBinding.ModelField(field.ToModelFieldInput(_modelType));
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
                return ValidationFieldBinding.ModelField(field.ToModelFieldInput(_modelType));

            throw new InvalidOperationException(
                $"Validation field '{fieldPath.Value}' is read by a browser rule's condition or comparison for model '{_modelType.FullName}', " +
                "but the validator's browser rules do not name it, so its shape is unknown.");
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
                var renderedItem = RenderedItemFieldMatch.Find(
                    collectionPath,
                    itemField.FieldName,
                    registeredPath);
                if (renderedItem == null)
                    continue;

                var renderedField = itemField.RenderedAt(renderedItem.ItemPrefix);

                if (renderedItem.IsDirectField && renderedField.HasRules)
                    yield return renderedField;

                foreach (var nestedItemField in ExpandRenderedItemFields(
                             renderedField.FieldName,
                             renderedField.ItemFields,
                             registeredPath))
                    yield return nestedItemField;
            }
        }

        private sealed class RenderedItemFieldMatch
        {
            private RenderedItemFieldMatch(ValidationFieldPath itemPrefix, bool isDirectField)
            {
                ItemPrefix = itemPrefix;
                IsDirectField = isDirectField;
            }

            internal ValidationFieldPath ItemPrefix { get; }
            internal bool IsDirectField { get; }

            internal static RenderedItemFieldMatch? Find(
                string collectionPath,
                string itemFieldPath,
                string registeredPath)
            {
                var openingBracket = collectionPath.Length;
                if (!registeredPath.StartsWith(collectionPath + "[", StringComparison.Ordinal))
                    return null;

                var closingBracket = registeredPath.IndexOf(']', openingBracket + 1);
                if (closingBracket < 0)
                    return null;

                var itemPrefix = registeredPath.Substring(0, closingBracket + 1);
                var renderedItemFieldPath = itemPrefix + "." + itemFieldPath;
                var directField = string.Equals(registeredPath, renderedItemFieldPath, StringComparison.Ordinal);
                if (directField)
                    return new RenderedItemFieldMatch(ValidationFieldPath.Of(itemPrefix), true);

                var nestedField =
                    registeredPath.StartsWith(renderedItemFieldPath + ".", StringComparison.Ordinal) ||
                    registeredPath.StartsWith(renderedItemFieldPath + "[", StringComparison.Ordinal);
                if (!nestedField)
                    return null;

                return new RenderedItemFieldMatch(ValidationFieldPath.Of(itemPrefix), false);
            }
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
        private readonly Shape _shape;

        private ValidationFieldBinding(ComponentId componentId, Shape shape)
        {
            _componentId = componentId;
            _shape = shape;
        }

        internal string ComponentId => _componentId.Value;

        internal Shape ShapeForValidation => _shape;

        // A rule reads every input as MVC binds it, through the model every plan of the composition
        // joins, so the browser judges the value the server will (grammar.md 2.1).
        internal ValueExpression ReadValue() => ValueExpression.ReadBound(ComponentId, ShapeForValidation);

        internal FieldComparisonTarget ReadConditionTarget() =>
            FieldComparisonTarget.ForComponentValue(ValueExpression.ReadBound(ComponentId, Shape.None), ShapeForValidation);

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
            return new ValidationFieldBinding(Alis.Reactive.PlanModel.ComponentId.Of(registration.ComponentId), registration.Shape);
        }

        internal static ValidationFieldBinding ModelField(ModelFieldInput field)
        {
            // Another plan renders the field; its value is read through the model all the same.
            return new ValidationFieldBinding(field.ComponentId, field.Shape);
        }
    }

    internal sealed class ModelFieldInput
    {
        private ModelFieldInput(ComponentId componentId, Shape shape)
        {
            ComponentId = componentId;
            Shape = shape;
        }

        internal ComponentId ComponentId { get; }
        internal Shape Shape { get; }

        internal static ModelFieldInput For(
            Type modelType,
            ValidationFieldPath fieldPath,
            Shape shape)
        {
            return new ModelFieldInput(
                Alis.Reactive.PlanModel.ComponentId.Of(IdGenerator.For(modelType, fieldPath.Value)),
                shape);
        }
    }
}
