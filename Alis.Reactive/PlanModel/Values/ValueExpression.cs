using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Alis.Reactive.Grammar;
using Alis.Reactive.PlanModel.Writing;
using Alis.Reactive.Members;

namespace Alis.Reactive.PlanModel.Values
{
    /// <summary>
    /// One value the plan reads: a literal, a member of an object (a component, a plugin, an element,
    /// data a callback was handed), the URL, an input as MVC binds it, a record, a list or an array
    /// operation. Each writes the grammar expression that reads it, in the scope it is used in.
    /// </summary>
    internal abstract class ValueExpression
    {
        private protected ValueExpression() { }

        internal abstract Shape OutputShape { get; }

        /// <summary>The value as it arrived, unconverted, for text shown as the server or the event sent it.</summary>
        internal virtual ValueExpression AsArrived() => this;

        /// <summary>Whether reading it waits for a promise, so a condition over it is asked in its own <c>if</c>.</summary>
        internal virtual bool ReadsAwaited => false;

        internal abstract Expr Write(Scope scope);

        /// <summary>The value as a comparison reads it: converted to the shape; a literal is written in it.</summary>
        internal virtual Expr WriteAs(Shape shape, Scope scope) => G.As(Write(scope), shape);

        // ── literals ─────────────────────────────────────────────────────────────

        internal static ValueExpression Literal(bool value) => new LiteralExpression(value, Shape.Boolean);

        internal static ValueExpression Literal(string value) => new LiteralExpression(value, Shape.String);

        internal static ValueExpression Literal(int value) => new LiteralExpression(value, Shape.Number);

        internal static ValueExpression Literal(DateTime value) => new LiteralExpression(value.ToString("O"), Shape.Date);

        internal static ValueExpression Null() => new LiteralExpression(null, Shape.None);

        private static ValueExpression LiteralRaw(object? value, Shape shape) => new LiteralExpression(value, shape);

        internal static ValueExpression LiteralFor(object? value, Shape shape)
        {
            if (value == null) return Null();
            if (value is DateTime date) return Literal(date);
            if (value is Field field) return Literal(field.Name);
            if (value is Enum option) return LiteralRaw(JavaScriptTextOf(option), shape);
            return LiteralRaw(value, shape);
        }

        // An option is its member name ("Descending"), unless the enum names the JavaScript text the
        // way System.Text.Json does: [JsonStringEnumMemberName("notequal")] NotEqual.
        private static string JavaScriptTextOf(Enum option)
        {
            var memberName = option.ToString();
            var member = option.GetType().GetField(memberName);
            var declared = member == null
                ? null
                : (JsonStringEnumMemberNameAttribute?)Attribute.GetCustomAttribute(member, typeof(JsonStringEnumMemberNameAttribute));
            return declared == null ? memberName : declared.Name;
        }

        // ── reads ────────────────────────────────────────────────────────────────

        /// <summary>A property of an object, or a path into data; with no path, the object or data itself.</summary>
        internal static ValueExpression Get(Target target, IReadOnlyList<string> path, Shape shape) =>
            new GetExpression(target, path, shape);

        internal static ValueExpression Get(Target target, Shape shape) =>
            new GetExpression(target, G.NoPath, shape);

        /// <summary>What a method of an object returns.</summary>
        internal static CallExpression Call(
            Target target,
            IReadOnlyList<string> path,
            IReadOnlyList<ValueExpression> args,
            IReadOnlyList<Shape> argShapes,
            Shape shape,
            string member) =>
            new CallExpression(target, path, args, argShapes, shape, member);

        internal static ValueExpression MethodResult(CallExpression call, string member, bool awaits) =>
            new MethodResultExpression(call, member, awaits);

        /// <summary>A query string key, as MVC binds it: a scalar from its first value, an array from all of them.</summary>
        internal static ValueExpression ReadUrl(string key, Shape shape) => new UrlRead(key, shape);

        /// <summary>An input's value as MVC binds it, through the model every plan of the composition joins.</summary>
        internal static ValueExpression ReadBound(string inputId, Shape shape) => new BoundRead(inputId, shape);

        // ── records and lists ────────────────────────────────────────────────────

        /// <summary>A record of named fields, written in the order given; a name appears once.</summary>
        internal static ValueExpression Record(IReadOnlyList<(string Name, ValueExpression Value)> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            var shapes = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var (name, value) in fields)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Object field name must not be null or whitespace.", nameof(fields));
                if (value == null) throw new ArgumentException($"Object field '{name}' must have a value.", nameof(fields));
                if (shapes.ContainsKey(name)) throw new ArgumentException($"Object field '{name}' is named twice.", nameof(fields));
                shapes.Add(name, value.OutputShape);
            }

            return new RecordExpression(fields.ToList(), Shape.ObjectOf(shapes));
        }

        internal static ValueExpression List(IReadOnlyList<ValueExpression> items, Shape shape)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Any(item => item == null)) throw new ArgumentException("Array item must not be null.", nameof(items));
            return new ListExpression(items.ToList(), shape);
        }

        // ── operators and tests, per item of an array lambda, which never waits ──

        /// <summary>An operator of grammar.md 2.2 over values (<c>??</c>, <c>?:</c>, a date's month counted from 1), its result read in a shape.</summary>
        internal static ValueExpression Operation(Operator op, Shape shape, params ValueExpression[] operands)
        {
            if (op == null) throw new ArgumentNullException(nameof(op));
            return new OperationExpression(op, operands, shape);
        }
    }

    internal sealed class LiteralExpression : ValueExpression
    {
        internal LiteralExpression(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal object? Value { get; }

        internal Shape Shape { get; }

        internal override Shape OutputShape => Shape;

        internal override Expr Write(Scope scope) => G.Lit(Value, Shape);

        internal override Expr WriteAs(Shape shape, Scope scope) => G.Lit(Value, shape);
    }

    internal sealed class GetExpression : ValueExpression
    {
        private readonly Target _target;
        private readonly IReadOnlyList<string> _path;
        private readonly Shape _shape;

        internal GetExpression(Target target, IReadOnlyList<string> path, Shape shape)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => _shape;

        // Data a callback received is shown as the server or the event sent it; an object's member, the URL
        // and a method read keep their type, which decides how they are read.
        internal override ValueExpression AsArrived() =>
            _target.IsData ? new GetExpression(_target, _path, Shape.None) : this;

        internal override Expr Write(Scope scope)
        {
            var (root, prefix) = _target.Write(scope);
            var path = prefix.Concat(_path).ToList();
            return path.Count == 0 ? G.As(root, _shape) : G.Get(root, path, _shape);
        }
    }

    internal sealed class CallExpression : ValueExpression
    {
        private readonly Target _target;
        private readonly IReadOnlyList<string> _path;
        private readonly IReadOnlyList<ValueExpression> _args;
        private readonly IReadOnlyList<Shape> _argShapes;
        private readonly Shape _shape;
        private readonly string _member;

        internal CallExpression(
            Target target,
            IReadOnlyList<string> path,
            IReadOnlyList<ValueExpression> args,
            IReadOnlyList<Shape> argShapes,
            Shape shape,
            string member)
        {
            _target = target ?? throw new ArgumentNullException(nameof(target));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _argShapes = argShapes ?? throw new ArgumentNullException(nameof(argShapes));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            _member = member ?? throw new ArgumentNullException(nameof(member));
        }

        internal override Shape OutputShape => _shape;

        internal override bool ReadsAwaited => _args.Any(arg => arg.ReadsAwaited);

        internal override Expr Write(Scope scope) => WriteCall(scope, _shape);

        /// <summary>The call with its result as it arrives: what a <c>let</c> binds, converted where the name is read.</summary>
        internal Expr WriteUnconverted(Scope scope) => WriteCall(scope, Shape.None);

        private Expr WriteCall(Scope scope, Shape shape)
        {
            var (root, prefix) = _target.Write(scope);
            var args = _args.Select(arg => arg.Write(scope)).ToList();
            return G.Call(root, prefix.Concat(_path).ToList(), args, _argShapes, shape);
        }
    }

    /// <summary>
    /// What a method returned: bound once per block, by a <c>let</c> hoisted before the statement that
    /// first reads it, and read by name after, converted to the method's shape where it is read. A promise
    /// is awaited by that <c>let</c> (grammar.md 3.1).
    /// </summary>
    internal sealed class MethodResultExpression : ValueExpression
    {
        private readonly CallExpression _call;
        private readonly string _member;
        private readonly bool _awaits;

        internal MethodResultExpression(CallExpression call, string member, bool awaits)
        {
            _call = call ?? throw new ArgumentNullException(nameof(call));
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _awaits = awaits;
        }

        internal override Shape OutputShape => _call.OutputShape;

        internal override bool ReadsAwaited => _awaits || _call.ReadsAwaited;

        internal override Expr Write(Scope scope) =>
            G.As(scope.Result(this, JsonMemberName.Of(_member), _awaits, () => _call.WriteUnconverted(scope)), OutputShape);
    }

    internal sealed class UrlRead : ValueExpression
    {
        private readonly string _key;
        private readonly Shape _shape;

        internal UrlRead(string key, Shape shape)
        {
            _key = key ?? throw new ArgumentNullException(nameof(key));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => _shape;

        // MVC binds a scalar from a key's first value and an array from all of them.
        internal override Expr Write(Scope scope)
        {
            var values = _shape.DescribesArray ? "queryAll" : "query";
            return G.Get(G.Get(G.Obj(scope.Objects.Document()), new[] { values }, Shape.None), new[] { _key }, _shape);
        }

    }

    internal sealed class BoundRead : ValueExpression
    {
        private readonly string _inputId;
        private readonly Shape _shape;

        internal BoundRead(string inputId, Shape shape)
        {
            _inputId = inputId ?? throw new ArgumentNullException(nameof(inputId));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => _shape;

        // The model hands the value back as MVC binds it, already in the input's shape (grammar.md M5): the
        // plan converts it no second time.
        internal override Expr Write(Scope scope) =>
            G.Call(G.Obj(scope.Objects.Model()), new[] { "boundValueOf" }, new Expr[] { G.Lit(_inputId) }, Shape.None);
    }

    internal sealed class RecordExpression : ValueExpression
    {
        private readonly IReadOnlyList<(string Name, ValueExpression Value)> _fields;
        private readonly Shape _shape;

        internal RecordExpression(IReadOnlyList<(string Name, ValueExpression Value)> fields, Shape shape)
        {
            _fields = fields;
            _shape = shape;
        }

        internal override Shape OutputShape => _shape;

        internal override bool ReadsAwaited => _fields.Any(member => member.Value.ReadsAwaited);

        internal override Expr Write(Scope scope) =>
            G.Record(_fields.Select(member => new RecordField(member.Name, member.Value.Write(scope))));
    }

    internal sealed class ListExpression : ValueExpression
    {
        internal ListExpression(IReadOnlyList<ValueExpression> items, Shape shape)
        {
            Items = items;
            Shape = shape;
        }

        internal IReadOnlyList<ValueExpression> Items { get; }

        internal Shape Shape { get; }

        internal override Shape OutputShape => Shape;

        internal override bool ReadsAwaited => Items.Any(item => item.ReadsAwaited);

        internal override Expr Write(Scope scope) => G.List(Items.Select(item => item.Write(scope)));
    }

    internal sealed class OperationExpression : ValueExpression
    {
        private readonly Operator _op;
        private readonly IReadOnlyList<ValueExpression> _operands;
        private readonly Shape _shape;

        internal OperationExpression(Operator op, IReadOnlyList<ValueExpression> operands, Shape shape)
        {
            _op = op;
            _operands = operands;
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => _shape;

        internal override Expr Write(Scope scope) =>
            G.As(G.Op(_op, _operands.Select(operand => operand.Write(scope)).ToArray()), _shape);
    }
}
