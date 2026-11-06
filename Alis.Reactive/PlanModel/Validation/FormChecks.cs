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

        internal static IEnumerable<CheckedField> Bind(
            IReadOnlyList<BrowserField> fields,
            string fieldPrefix,
            Type? model)
        {
            var values = new FormFieldValues(fieldPrefix, new ModelDefaults(model));
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
        private readonly string _fieldPrefix;
        private readonly ModelDefaults _defaults;

        internal FormFieldValues(string fieldPrefix, ModelDefaults defaults)
        {
            _fieldPrefix = fieldPrefix;
            _defaults = defaults;
        }

        internal const string Indexes = "i";

        internal const string Sent = "sent";

        internal override ValueExpression Value(FieldReference field) => Read(field.Path, field.Shape, G.Ref(Indexes));

        internal Expr Items(FieldReference field, Scope scope)
        {
            var path = FieldPath.Of(field.Path);
            var tuples = G.List(new[] { G.List(Array.Empty<Expr>()) });
            var level = 0;
            for (var position = 0; position < path.Count; position++)
            {
                if (!path[position].IsEachItem) continue;
                level++;
                var owner = "o" + level;
                var list = FieldPath.Text(path.Take(position));
                var items = ItemsOf(list, G.Ref(owner), scope, level);
                tuples = G.Call(tuples, PathMembers.Required, "flatMap", G.Fn(items, owner));
            }

            return tuples;
        }

        internal Expr ItemIndexes(FieldReference collection, Scope scope)
        {
            var depth = FieldPath.ItemsIn(collection.Path);
            var owner = G.Call(G.Ref(Indexes), PathMembers.Required, "slice", G.Lit(0), G.Lit(depth));
            return ItemsOf(collection.Path, owner, scope, depth + 1);
        }

        internal CheckedField Check(BrowserField field)
            => new CheckedField(NameOf(field.Reference.Path), field.Reference, field.Rules, this);

        private Expr ItemsOf(string list, Expr owner, Scope scope, int level)
        {
            var read = Read(list, ClrShape.Of(_defaults.TypeOf(list)), owner).Write(scope);
            var item = "x" + level;
            var position = "k" + level;
            var tuple = G.Call(owner, PathMembers.Required, "concat", G.List(new[] { G.Ref(position) }));
            return G.Call(G.Op(Operator.Coalesce, read, G.List(Array.Empty<Expr>())), PathMembers.Required, "map", G.Fn(tuple, item, position));
        }

        private FormRead Read(string path, Shape shape, Expr indexes)
            => new FormRead(NameOf(path), shape, indexes, _defaults.For(path, NameOf));

        private string NameOf(string path) => HtmlFieldPrefix.Join(_fieldPrefix, path);
    }
}
