using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
{
    /// <summary>
    /// One input a form checks: the element that holds it, the name the server reports its errors under,
    /// the type the model binds it in, and the validator's rules on it.
    /// </summary>
    internal sealed class CheckedField
    {
        internal CheckedField(string componentId, string serverName, Shape shape, IReadOnlyList<BrowserRule> rules, FormFieldValues values)
        {
            ComponentId = componentId;
            ServerName = serverName;
            Shape = shape;
            Rules = rules;
            Values = values;
        }

        internal string ComponentId { get; }

        internal string ServerName { get; }

        internal CheckedField With(IReadOnlyList<BrowserRule> rules) =>
            new CheckedField(ComponentId, ServerName, Shape, Rules.Concat(rules).ToArray(), Values);

        internal Shape Shape { get; }

        internal IReadOnlyList<BrowserRule> Rules { get; }

        /// <summary>Where the rules' conditions and comparisons read the other members.</summary>
        internal FormFieldValues Values { get; }

        /// <summary>The value the rules check: the input as MVC binds it.</summary>
        internal ValueExpression Value => ValueExpression.ReadBound(ComponentId, Shape);

        /// <summary>
        /// The inputs a validator's rules check on this plan's form: every member with rules, and each
        /// item of a collection the page rendered.
        /// </summary>
        internal static IEnumerable<CheckedField> Bind(
            IReadOnlyList<BrowserField> fields,
            IReadOnlyDictionary<string, RenderedInput> renderedInputs,
            Type modelType,
            string fieldPrefix)
        {
            // The checks are written relative to the validator's model; the view's field prefix is applied
            // where an input is resolved, so the rendered paths read here are relative to it too.
            var values = new FormFieldValues(renderedInputs, modelType, fieldPrefix);
            var renderedPaths = renderedInputs.Keys
                .Select(path => HtmlFieldPrefix.Strip(fieldPrefix, path))
                .Where(path => path != null)
                .Select(path => path!)
                .ToList();
            // A checked member is carried whether or not this plan renders it (a partial of the same model may)
            // and whether or not the browser can check it: a message the server sends for it then lands
            // beside its input wherever the input is.
            var bound = new List<CheckedField>();
            foreach (var field in fields)
            {
                if (field.IsChecked)
                    Join(bound, values.Check(field));

                foreach (var itemField in RenderedItemFields(field.FieldName, field.ItemFields, renderedPaths))
                    Join(bound, values.Check(itemField));
            }
            return bound;
        }

        // Several rules on one collection each name its items: an item's field carries the checks of all of them.
        private static void Join(List<CheckedField> bound, CheckedField field)
        {
            var index = bound.FindIndex(existing => existing.ComponentId == field.ComponentId);
            if (index < 0) bound.Add(field);
            else bound[index] = bound[index].With(field.Rules);
        }

        // Each item field of a collection, at every item the page rendered an input of: Lines[0].Qty for
        // the item field Qty when the page rendered Lines[0].Qty, or an input nested inside it.
        private static IEnumerable<BrowserField> RenderedItemFields(
            string collectionPath,
            IReadOnlyList<BrowserField> itemFields,
            IEnumerable<string> renderedPaths)
        {
            foreach (var renderedPath in renderedPaths)
                foreach (var itemField in itemFields)
                {
                    if (!TryRenderedItem(collectionPath, itemField.FieldName, renderedPath, out var itemPrefix, out var isDirectField))
                        continue;

                    var renderedField = itemField.RenderedAt(itemPrefix);
                    if (isDirectField && renderedField.IsChecked)
                        yield return renderedField;

                    foreach (var nestedItemField in RenderedItemFields(renderedField.FieldName, renderedField.ItemFields, new[] { renderedPath }))
                        yield return nestedItemField;
                }
        }

        private static bool TryRenderedItem(
            string collectionPath,
            string itemFieldPath,
            string renderedPath,
            out string itemPrefix,
            out bool isDirectField)
        {
            itemPrefix = "";
            isDirectField = false;

            var openingBracket = collectionPath.Length;
            if (!renderedPath.StartsWith(collectionPath + "[", StringComparison.Ordinal)) return false;

            var closingBracket = renderedPath.IndexOf(']', openingBracket + 1);
            if (closingBracket < 0) return false;

            var prefix = renderedPath.Substring(0, closingBracket + 1);
            var renderedItemFieldPath = itemFieldPath.Length == 0 ? prefix : prefix + "." + itemFieldPath;
            isDirectField = string.Equals(renderedPath, renderedItemFieldPath, StringComparison.Ordinal);
            var nestedField =
                renderedPath.StartsWith(renderedItemFieldPath + ".", StringComparison.Ordinal) ||
                renderedPath.StartsWith(renderedItemFieldPath + "[", StringComparison.Ordinal);
            if (!isDirectField && !nestedField) return false;

            itemPrefix = prefix;
            return true;
        }
    }

    /// <summary>
    /// The inputs a form's rules read, by the member's path under the view's field prefix: an input this
    /// plan rendered, by its MVC name, in the type the model binds it; otherwise the input a partial of the
    /// same model renders under the id the member gives it. Every input is read through the model, as MVC
    /// binds it.
    /// </summary>
    internal sealed class FormFieldValues
    {
        private readonly IReadOnlyDictionary<string, RenderedInput> _renderedInputs;
        private readonly Type _modelType;
        private readonly string _fieldPrefix;

        internal FormFieldValues(IReadOnlyDictionary<string, RenderedInput> renderedInputs, Type modelType, string fieldPrefix)
        {
            _renderedInputs = renderedInputs;
            _modelType = modelType;
            _fieldPrefix = fieldPrefix;
        }

        internal (string Id, Shape Shape) Resolve(FieldReference field)
        {
            var path = HtmlFieldPrefix.Join(_fieldPrefix, field.Path);
            return _renderedInputs.TryGetValue(path, out var rendered)
                ? (rendered.ComponentId, rendered.Shape)
                : (IdGenerator.For(_modelType, path), field.Shape);
        }

        internal ValueExpression Value(FieldReference field)
        {
            var input = Resolve(field);
            return ValueExpression.ReadBound(input.Id, input.Shape);
        }

        // The field is named as its input binds, under the view's prefix: the name the server reports it by.
        internal CheckedField Check(BrowserField field)
        {
            var input = Resolve(field.Reference);
            return new CheckedField(input.Id, HtmlFieldPrefix.Join(_fieldPrefix, field.FieldName), input.Shape, field.Rules, this);
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
