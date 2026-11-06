using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Alis.Reactive.Grammar;
using Alis.Reactive.Inputs;
using Alis.Reactive.Members;
using Alis.Reactive.PlanModel.Values;
using Alis.Reactive.PlanModel.Writing;
using Alis.Reactive.WireTerms;

namespace Alis.Reactive.PlanModel.Validation
{
    internal sealed class ModelDefaults
    {
        private readonly Type? _model;
        private readonly Dictionary<Type, object> _holders = new Dictionary<Type, object>();

        internal ModelDefaults(Type? model)
        {
            _model = model;
        }

        private Type Model => _model ?? throw new InvalidOperationException(
            "A form's browser rules read members of the model their rule set names, and this rule set names none.");

        internal Unsent For(string path, Func<string, string> nameOf)
        {
            var steps = FieldPath.Of(path);
            var levels = new List<UnsentLevel> { new UnsentLevel(null, Tree(Model, steps), steps.Count(step => step.IsEachItem)) };
            var type = Model;
            for (var position = 0; position < steps.Count; position++)
            {
                type = TypeIn(type, steps[position], path);
                if (!steps[position].IsEachItem) continue;

                var rest = steps.Skip(position + 1).ToList();
                if (rest.Count == 0) continue;

                var itemPattern = nameOf(FieldPath.Text(steps.Take(position + 1)));
                levels.Add(new UnsentLevel(itemPattern, Tree(type, rest), rest.Count(step => step.IsEachItem)));
            }

            return new Unsent(levels);
        }

        internal Type TypeOf(string path)
            => FieldPath.Of(path).Aggregate(Model, (holder, step) => TypeIn(holder, step, path));

        private ValueExpression Tree(Type holderType, IReadOnlyList<PathSegment> steps)
            => Project(Holder(holderType), holderType, steps, 0);

        private static ValueExpression Project(object? value, Type type, IReadOnlyList<PathSegment> steps, int position)
        {
            if (position == steps.Count) return Leaf(value, type);

            var step = steps[position];
            var next = TypeIn(type, step, FieldPath.Text(steps));
            if (value == null) return ValueExpression.Null();
            if (step.IsEachItem)
            {
                var items = ((IEnumerable)value).Cast<object?>().Select(item => Project(item, next, steps, position + 1)).ToList();
                return ValueExpression.List(items, Shape.ArrayOf(Shape.Any));
            }

            return Project(ValueIn(value, type, step), next, steps, position + 1);
        }

        private static Type TypeIn(Type holder, PathSegment step, string path)
            => step.IsEachItem || step.IsIndex ? ItemTypeOf(holder, path) : PropertyOf(holder, step, path).PropertyType;

        private static object? ValueIn(object holder, Type holderType, PathSegment step)
        {
            if (!step.IsIndex) return PropertyOf(holderType, step, step.ToPathText()).GetValue(holder);
            return holder is IList list && step.Index < list.Count ? list[step.Index] : null;
        }

        private static Type ItemTypeOf(Type list, string path)
            => FieldReference.ItemTypeOf(list)
            ?? throw new InvalidOperationException($"'{path}' reads the items of {CSharpTypeName.Of(list)}, which holds none.");

        private static PropertyInfo PropertyOf(Type holder, PathSegment step, string path)
            => holder.GetProperty(step.ToPathText(), BindingFlags.Public | BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                $"'{path}' names '{step.ToPathText()}', which {CSharpTypeName.Of(holder)} does not have as a public property.");

        private static ValueExpression Leaf(object? value, Type type)
        {
            if (value == null) return ValueExpression.Null();
            if (ClrShape.IsDataClass(value.GetType())) return ValueExpression.Record(Array.Empty<(string, ValueExpression)>());

            var shape = ClrShape.Of(type);
            if (!(value is string) && value is IEnumerable items && shape.DescribesArray)
            {
                var itemType = ItemTypeOf(type, CSharpTypeName.Of(type));
                return ValueExpression.List(items.Cast<object?>().Select(item => Leaf(item, itemType)).ToList(), Shape.ArrayOf(Shape.Any));
            }

            var hasNoPlanLiteral = ClrShape.Of(value.GetType()).Equals(Shape.Any) && !(value is IEnumerable);
            return hasNoPlanLiteral ? ValueExpression.Literal(WireTextOf(value)) : ValueExpression.LiteralFor(value, shape);
        }

        private static string WireTextOf(object value)
        {
            var written = JsonSerializer.SerializeToElement(value, value.GetType());
            if (written.ValueKind == JsonValueKind.String) return written.GetString()!;
            throw new InvalidOperationException(
                $"A form's check reads a member its request does not send as the endpoint binds it, and the endpoint writes the " +
                $"{CSharpTypeName.Of(value.GetType())} it starts with as {written.ValueKind}, not text the browser reads. Start the " +
                "member at null, or send it.");
        }

        private object Holder(Type type)
        {
            if (_holders.TryGetValue(type, out var made)) return made;

            var holder = New(type);
            _holders.Add(type, holder);
            return holder;
        }

        private object New(Type type)
        {
            if (type.IsValueType) return Activator.CreateInstance(type)!;

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            var parameterless = constructors.FirstOrDefault(constructor => constructor.GetParameters().Length == 0);
            var bindable = parameterless ?? (constructors.Length == 1 ? constructors[0] : null);
            if (type.IsAbstract || bindable == null)
            {
                throw new InvalidOperationException(
                    $"A form's check reads a member its request does not send as a new {CSharpTypeName.Of(type)} leaves it, but " +
                    $"{CSharpTypeName.Of(type)} has neither a public parameterless constructor nor a single public constructor, so " +
                    $"the endpoint cannot bind one into {CSharpTypeName.Of(Model)} either. Give {CSharpTypeName.Of(type)} a public " +
                    "parameterless constructor.");
            }

            try
            {
                return bindable.Invoke(bindable.GetParameters().Select(DefaultOf).ToArray());
            }
            catch (TargetInvocationException thrown)
            {
                throw new InvalidOperationException(
                    $"A new {CSharpTypeName.Of(type)}, made to read the members a request for {CSharpTypeName.Of(Model)} does not send, threw: " +
                    thrown.InnerException?.Message, thrown.InnerException);
            }
        }

        private static object? DefaultOf(ParameterInfo parameter)
            => parameter.HasDefaultValue ? parameter.DefaultValue
            : parameter.ParameterType.IsValueType ? Activator.CreateInstance(parameter.ParameterType)
            : null;
    }

    internal sealed class Unsent
    {
        private readonly IReadOnlyList<UnsentLevel> _levels;

        internal Unsent(IReadOnlyList<UnsentLevel> levels)
        {
            _levels = levels;
        }

        internal Expr Write(Scope scope, Expr indexes, Func<string, Expr> sends)
        {
            var read = _levels[0].Write(scope, indexes);
            foreach (var level in _levels.Skip(1))
                read = G.Op(Operator.Cond, sends(level.ItemPattern!), level.Write(scope, indexes), read);
            return read;
        }
    }

    internal sealed class UnsentLevel
    {
        private readonly ValueExpression _tree;
        private readonly int _itemsBelow;

        internal UnsentLevel(string? itemPattern, ValueExpression tree, int itemsBelow)
        {
            ItemPattern = itemPattern;
            _tree = tree;
            _itemsBelow = itemsBelow;
        }

        internal string? ItemPattern { get; }

        internal Expr Write(Scope scope, Expr indexes)
        {
            var read = _tree.Write(scope);
            var first = ItemPattern == null ? 0 : FieldPath.ItemsIn(ItemPattern);
            for (var position = first; position < first + _itemsBelow; position++)
                read = G.Call(read, PathMembers.Optional, "at", G.Call(indexes, PathMembers.Optional, "at", G.Lit(position)));
            return read;
        }
    }

    internal static class FieldPath
    {
        private const string EachItem = "[*]";

        internal static IReadOnlyList<PathSegment> Of(string path) => Path.Parse(path).Segments;

        internal static string Text(IEnumerable<PathSegment> steps)
        {
            var text = new System.Text.StringBuilder();
            foreach (var step in steps)
            {
                if (step.IsEachItem) text.Append(EachItem);
                else if (step.IsIndex) text.Append('[').Append(step.Index).Append(']');
                else text.Append(text.Length > 0 ? "." : "").Append(step.ToPathText());
            }

            return text.ToString();
        }

        internal static int ItemsIn(string pattern) => (pattern.Length - pattern.Replace(EachItem, "").Length) / EachItem.Length;
    }
}
