using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Inputs;
using Alis.Reactive.PlanModel.Conditions;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Validation
{
    /// <summary>
    /// One member, or the items of a collection, that a form checks: the binding name the server reports it under,
    /// the type the model binds it in, and the validator's rules on it. An item's name stands for every item,
    /// <c>Lines[*].Qty</c>, so the form checks each item mounted when it checks, whoever rendered it.
    /// </summary>
    internal sealed class CheckedField
    {
        internal CheckedField(string name, Shape shape, IReadOnlyList<BrowserRule> rules, FormFieldValues values)
        {
            Name = name;
            Shape = shape;
            Rules = rules;
            Values = values;
        }

        internal string Name { get; }

        internal Shape Shape { get; }

        internal IReadOnlyList<BrowserRule> Rules { get; }

        /// <summary>Where the rules' conditions and comparisons read the other members.</summary>
        internal FormFieldValues Values { get; }

        internal CheckedField With(IReadOnlyList<BrowserRule> rules) =>
            new CheckedField(Name, Shape, Rules.Concat(rules).ToArray(), Values);

        /// <summary>
        /// The fields a validator's rules check on a form: every member with rules, whether or not this plan renders
        /// it (a partial or the member's editor template may), and each item field of a collection, named for every item.
        /// </summary>
        internal static IEnumerable<CheckedField> Bind(string formId, IReadOnlyList<BrowserField> fields, string fieldPrefix)
        {
            var values = new FormFieldValues(formId, fieldPrefix);
            var bound = new List<CheckedField>();
            foreach (var field in Checked(fields))
                Join(bound, values.Check(field));
            return bound;
        }

        private static IEnumerable<BrowserField> Checked(IEnumerable<BrowserField> fields)
        {
            foreach (var field in fields)
            {
                if (field.IsChecked) yield return field;

                var items = field.ItemFields.Select(itemField => itemField.UnderItem(field.FieldName + "[*]"));
                foreach (var itemField in Checked(items))
                    yield return itemField;
            }
        }

        private static void Join(List<CheckedField> bound, CheckedField field)
        {
            var index = bound.FindIndex(existing => existing.Name == field.Name);
            if (index < 0) bound.Add(field);
            else bound[index] = bound[index].With(field.Rules);
        }
    }

    /// <summary>
    /// The members a form's rules read, by the binding name the server reports each under, the view's field prefix
    /// applied: each is read from the input that binds it, whichever view rendered it (this plan, a partial, a
    /// member's editor template, a slot's content), as MVC binds it.
    /// </summary>
    internal sealed class FormFieldValues : FieldValues
    {
        private readonly string _formId;
        private readonly string _fieldPrefix;

        internal FormFieldValues(string formId, string fieldPrefix)
        {
            _formId = formId;
            _fieldPrefix = fieldPrefix;
        }

        internal const string Indexes = "i";

        internal override ValueExpression Value(FieldReference field) => ValueExpression.ReadBound(NameOf(field), field.Shape, _formId, Indexes);

        internal CheckedField Check(BrowserField field) =>
            new CheckedField(NameOf(field.Reference), field.Reference.Shape, field.Rules, this);

        private string NameOf(FieldReference field) => HtmlFieldPrefix.Join(_fieldPrefix, field.Path);
    }
}
