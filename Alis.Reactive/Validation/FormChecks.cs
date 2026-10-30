using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive.Validation
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

        // Each checked member, then each item field of a collection at the item name standing for every item: Lines[*].Qty
        // for the item field Qty of Lines, and Lines[*].Deliveries[*].Code for an item's own collection.
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

        // Several rules on one collection each name its items: an item's field carries the checks of all of them.
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
    internal sealed class FormFieldValues
    {
        private readonly string _formId;
        private readonly string _fieldPrefix;

        internal FormFieldValues(string formId, string fieldPrefix)
        {
            _formId = formId;
            _fieldPrefix = fieldPrefix;
        }

        internal ValueExpression Value(FieldReference field) => new FormInputRead(_formId, NameOf(field), field.Shape);

        internal CheckedField Check(BrowserField field) =>
            new CheckedField(NameOf(field.Reference), field.Reference.Shape, field.Rules, this);

        private string NameOf(FieldReference field) => HtmlFieldPrefix.Join(_fieldPrefix, field.Path);
    }

    /// <summary>
    /// A member a form's test reads, by its binding name, as MVC binds it: the form's own input, else the page's
    /// (grammar.md M8). An item's name, <c>Lines[*].Qty</c>, is read at the indexes of the item the test checks,
    /// which the form hands each test as <see cref="Indexes"/>.
    /// </summary>
    internal sealed class FormInputRead : ValueExpression
    {
        /// <summary>The parameter a test's check and condition take the indexes of the item it checks by.</summary>
        internal const string Indexes = "i";

        private readonly string _formId;
        private readonly string _name;
        private readonly Shape _shape;

        internal FormInputRead(string formId, string name, Shape shape)
        {
            _formId = formId;
            _name = name;
            _shape = shape;
        }

        internal override Shape OutputShape => _shape;

        // The model hands the value back as MVC binds it, already in the input's shape: the plan converts it no
        // second time.
        internal override Expr Write(Scope scope) =>
            G.Call(G.Obj(scope.Objects.Inputs(_formId)), new[] { "valueOf" }, new Expr[] { G.Lit(_name), G.Ref(Indexes) }, Shape.None);
    }
}
