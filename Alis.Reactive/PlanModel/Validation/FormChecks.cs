using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Grammar;
using Alis.Reactive.Inputs;
using Alis.Reactive.Members;
using Alis.Reactive.PlanModel.Conditions;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;

namespace Alis.Reactive.PlanModel.Validation
{
    internal sealed class CheckedField
    {
        internal CheckedField(string name, FieldReference reference, IReadOnlyList<BrowserRule> rules, FormFieldValues values)
        {
            Name = name;
            Reference = reference;
            Rules = rules;
            Values = values;
        }

        internal string Name { get; }

        internal FieldReference Reference { get; }

        internal Shape Shape => Reference.Shape;

        internal IReadOnlyList<BrowserRule> Rules { get; }

        internal FormFieldValues Values { get; }

        internal CheckedField With(IReadOnlyList<BrowserRule> rules)
            => new CheckedField(Name, Reference, Rules.Concat(rules).ToArray(), Values);

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
                if (field.Rules.Count > 0) yield return field;

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

        internal override ValueExpression Value(FieldReference field)
            => IsListOrObject(field) ? Sent(field) : ValueExpression.ReadBound(NameOf(field), field.Shape, _formId, Indexes);

        internal Expr Checked(FieldReference field, Expr entered, Scope scope)
            => IsListOrObject(field) ? Sent(field).Write(scope) : entered;

        internal ValueExpression ItemIndexes(FieldReference item)
            => ValueExpression.ReadBoundItemIndexes(NameOf(item), _formId, Indexes, OwnerDepthOf(item));

        internal CheckedField Check(BrowserField field)
            => new CheckedField(NameOf(field.Reference), field.Reference, field.Rules, this);

        private ValueExpression Sent(FieldReference field) => ValueExpression.ReadSent(NameOf(field), field.Shape, _formId, Indexes);

        private static bool IsListOrObject(FieldReference field) => field.Shape.DescribesArray || ClrShape.IsDataClass(field.Type);

        private string NameOf(FieldReference field) => HtmlFieldPrefix.Join(_fieldPrefix, field.Path);

        private static int OwnerDepthOf(FieldReference item) => item.Path.Split(new[] { "[*]" }, StringSplitOptions.None).Length - 2;
    }
}
