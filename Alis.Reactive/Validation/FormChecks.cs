using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One input a form checks: the element that holds it, the name the server reports its errors under,
    /// the type the model binds it in, and the validator's rules on it.
    /// </summary>
    internal sealed class CheckedField
    {
        internal CheckedField(string componentId, string serverName, Shape shape, IReadOnlyList<ClientRule> rules, FormFieldValues values)
        {
            ComponentId = componentId;
            ServerName = serverName;
            Shape = shape;
            Rules = rules;
            Values = values;
        }

        internal string ComponentId { get; }

        internal string ServerName { get; }

        internal Shape Shape { get; }

        internal IReadOnlyList<ClientRule> Rules { get; }

        /// <summary>Where the rules' conditions and comparisons read the other members.</summary>
        internal FormFieldValues Values { get; }

        /// <summary>The value the rules check: the input as MVC binds it.</summary>
        internal ValueExpression Value => ValueExpression.ReadBound(ComponentId, Shape);

        /// <summary>
        /// The inputs a validator's rules check on this plan's form: every member with rules, and each
        /// item of a collection the page rendered.
        /// </summary>
        internal static IEnumerable<CheckedField> Bind(
            IReadOnlyList<ClientValidationField> fields,
            IReadOnlyDictionary<string, ComponentRegistration> registeredInputs,
            Type modelType)
        {
            var values = new FormFieldValues(registeredInputs, modelType);
            foreach (var field in fields)
            {
                if (field.HasRules)
                    yield return values.Check(field);

                foreach (var itemField in RenderedItemFields(field.FieldName, field.ItemFields, registeredInputs.Keys))
                    yield return values.Check(itemField);
            }
        }

        // Each item field of a collection, at every item the page rendered an input of: Lines[0].Qty for
        // the item field Qty when the page rendered Lines[0].Qty, or an input nested inside it.
        private static IEnumerable<ClientValidationField> RenderedItemFields(
            string collectionPath,
            IReadOnlyList<ClientValidationField> itemFields,
            IEnumerable<string> registeredPaths)
        {
            foreach (var registeredPath in registeredPaths)
                foreach (var itemField in itemFields)
                {
                    if (!TryRenderedItem(collectionPath, itemField.FieldName, registeredPath, out var itemPrefix, out var isDirectField))
                        continue;

                    var renderedField = itemField.RenderedAt(itemPrefix);
                    if (isDirectField && renderedField.HasRules)
                        yield return renderedField;

                    foreach (var nestedItemField in RenderedItemFields(renderedField.FieldName, renderedField.ItemFields, new[] { registeredPath }))
                        yield return nestedItemField;
                }
        }

        private static bool TryRenderedItem(
            string collectionPath,
            string itemFieldPath,
            string registeredPath,
            out ValidationFieldPath itemPrefix,
            out bool isDirectField)
        {
            itemPrefix = ValidationFieldPath.Empty;
            isDirectField = false;

            var openingBracket = collectionPath.Length;
            if (!registeredPath.StartsWith(collectionPath + "[", StringComparison.Ordinal)) return false;

            var closingBracket = registeredPath.IndexOf(']', openingBracket + 1);
            if (closingBracket < 0) return false;

            var prefix = registeredPath.Substring(0, closingBracket + 1);
            var renderedItemFieldPath = prefix + "." + itemFieldPath;
            isDirectField = string.Equals(registeredPath, renderedItemFieldPath, StringComparison.Ordinal);
            var nestedField =
                registeredPath.StartsWith(renderedItemFieldPath + ".", StringComparison.Ordinal) ||
                registeredPath.StartsWith(renderedItemFieldPath + "[", StringComparison.Ordinal);
            if (!isDirectField && !nestedField) return false;

            itemPrefix = ValidationFieldPath.Of(prefix);
            return true;
        }
    }

    /// <summary>
    /// The inputs a form's rules read, by the member's path: an input this plan rendered, by its MVC name,
    /// in the type the model binds it; otherwise the input a partial renders under the id the model's
    /// member gives it. Every input is read through the model, as MVC binds it.
    /// </summary>
    internal sealed class FormFieldValues
    {
        private readonly IReadOnlyDictionary<string, ComponentRegistration> _registeredInputs;
        private readonly Type _modelType;

        internal FormFieldValues(IReadOnlyDictionary<string, ComponentRegistration> registeredInputs, Type modelType)
        {
            _registeredInputs = registeredInputs;
            _modelType = modelType;
        }

        internal (string Id, Shape Shape) Resolve(FieldReference field) =>
            _registeredInputs.TryGetValue(field.Path.Value, out var registration)
                ? (registration.ComponentId, registration.Shape)
                : (IdGenerator.For(_modelType, field.Path.Value), field.Shape);

        internal ValueExpression Value(FieldReference field)
        {
            var input = Resolve(field);
            return ValueExpression.ReadBound(input.Id, input.Shape);
        }

        internal CheckedField Check(ClientValidationField field)
        {
            var input = Resolve(field.Reference);
            return new CheckedField(input.Id, field.FieldName, input.Shape, field.Rules, this);
        }
    }

    /// <summary>
    /// The inputs one form checks. A second request that validates the same form replaces an input's
    /// rules and adds the inputs it newly names.
    /// </summary>
    internal sealed class FormChecks
    {
        private readonly List<CheckedField> _fields = new List<CheckedField>();

        internal IReadOnlyList<CheckedField> Fields => _fields;

        internal void Merge(IEnumerable<CheckedField> incoming)
        {
            foreach (var field in incoming)
            {
                var index = _fields.FindIndex(existing => existing.ComponentId == field.ComponentId);
                if (index < 0) _fields.Add(field);
                else _fields[index] = field;
            }
        }
    }
}
