using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.PlanModel
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

        // ── literals ─────────────────────────────────────────────────────────────

        internal static ValueExpression Literal(bool value) => new LiteralExpression(value, Shape.Boolean);

        internal static ValueExpression Literal(string value) => new LiteralExpression(value, Shape.String);

        internal static ValueExpression Literal(int value) => new LiteralExpression(value, Shape.Number);

        internal static ValueExpression Literal(DateTime value) => new LiteralExpression(value.ToString("O"), Shape.Date);

        internal static ValueExpression Null() => new LiteralExpression(null, Shape.None);

        internal static ValueExpression LiteralRaw(object? value, Shape shape) => new LiteralExpression(value, shape);

        internal static ValueExpression LiteralFor(object? value, Shape shape)
        {
            if (value == null) return Null();
            if (value is DateTime date) return Literal(date);
            if (value is Field field) return Literal(field.Name);
            if (value is Enum option) return LiteralRaw(JavaScriptTextOf(option), shape);
            return LiteralRaw(value, shape);
        }

        internal static ValueExpression LiteralFromValue(object? value) =>
            value == null ? Null() : LiteralRaw(value, Shape.FromValue(value));

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
        internal static ValueExpression Read(Receiver receiver, IReadOnlyList<string> path, Shape shape) =>
            new ReadExpression(receiver, path, shape);

        internal static ValueExpression ReadWhole(Receiver receiver, Shape shape) =>
            new ReadExpression(receiver, G.NoPath, shape);

        /// <summary>What a method of an object returns.</summary>
        internal static ValueExpression Invoke(
            Receiver receiver,
            IReadOnlyList<string> path,
            IReadOnlyList<ValueExpression> args,
            IReadOnlyList<Shape> argShapes,
            Shape shape,
            string member) =>
            new InvokeExpression(receiver, path, args, argShapes, shape, member);

        internal static ValueExpression MethodResult(ValueExpression call, string member, bool awaits) =>
            new MethodResultExpression(call, member, awaits);

        /// <summary>A query string key, as MVC binds it: a scalar from its first value, an array from all of them.</summary>
        internal static ValueExpression ReadUrl(string key, Shape shape) => new UrlRead(key, shape);

        /// <summary>An input's value as MVC binds it, through the model every plan of the composition joins.</summary>
        internal static ValueExpression ReadBound(string inputId, Shape shape) => new BoundRead(inputId, shape);

        // ── records and lists ────────────────────────────────────────────────────

        internal static ValueExpression Object(IReadOnlyDictionary<string, ValueExpression> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            var snapshot = new Dictionary<string, ValueExpression>(StringComparer.Ordinal);
            var shapes = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                if (string.IsNullOrWhiteSpace(field.Key))
                    throw new ArgumentException("Object field name must not be null or whitespace.", nameof(fields));
                snapshot[field.Key] = field.Value ?? throw new ArgumentException($"Object field '{field.Key}' must have a value.", nameof(fields));
                shapes[field.Key] = field.Value.OutputShape;
            }

            return new ObjectExpression(snapshot, Shape.ObjectOf(shapes));
        }

        internal static ValueExpression Array(IReadOnlyList<ValueExpression> items, Shape shape)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Any(item => item == null)) throw new ArgumentException("Array item must not be null.", nameof(items));
            return new ArrayExpression(items.ToList(), shape);
        }

        // ── array operations ─────────────────────────────────────────────────────

        internal static ValueExpression ArrayCount(ValueExpression source) =>
            new ArrayOperation("count", source, Shape.Number);

        internal static ValueExpression ArrayFilter(ValueExpression source, ConditionGraph predicate, Shape itemShape) =>
            new ArrayOperation("filter", source, Shape.ArrayOf(itemShape), predicate);

        internal static ValueExpression ArrayMap(ValueExpression source, ValueExpression projection, Shape resultItemShape) =>
            new ArrayOperation("map", source, Shape.ArrayOf(resultItemShape), projection: projection);

        internal static ValueExpression ArraySum(ValueExpression source, ValueExpression projection) =>
            new ArrayOperation("sum", source, Shape.Number, projection: projection);

        internal static ValueExpression ArrayAny(ValueExpression source, ConditionGraph? predicate) =>
            new ArrayOperation("any", source, Shape.Boolean, predicate);

        internal static ValueExpression ArrayAll(ValueExpression source, ConditionGraph predicate) =>
            new ArrayOperation("all", source, Shape.Boolean, predicate);

        internal static ValueExpression ArrayFind(
            ValueExpression source, ConditionGraph predicate, ValueExpression? projection, Shape resultShape) =>
            new ArrayOperation("find", source, resultShape, predicate, projection);

        internal static ValueExpression ArrayOrderBy(ValueExpression source, ValueExpression key, Shape itemShape, bool descending) =>
            new ArrayOperation(descending ? "orderByDescending" : "orderBy", source, Shape.ArrayOf(itemShape), projection: key);
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
    }

    internal sealed class ReadExpression : ValueExpression
    {
        private readonly Receiver _receiver;
        private readonly IReadOnlyList<string> _path;
        private readonly Shape _shape;

        internal ReadExpression(Receiver receiver, IReadOnlyList<string> path, Shape shape)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => _shape;

        // Data a callback received is shown as the server or the event sent it; an object's member, the URL
        // and a method read keep their type, which decides how they are read.
        internal override ValueExpression AsArrived() =>
            _receiver.IsData ? new ReadExpression(_receiver, _path, Shape.None) : this;

        internal override Expr Write(Scope scope)
        {
            var (root, prefix) = _receiver.Locate(scope);
            var path = prefix.Concat(_path).ToList();
            return path.Count == 0 ? G.As(root, _shape) : G.Get(root, path, _shape);
        }
    }

    internal sealed class InvokeExpression : ValueExpression
    {
        private readonly Receiver _receiver;
        private readonly IReadOnlyList<string> _path;
        private readonly IReadOnlyList<ValueExpression> _args;
        private readonly IReadOnlyList<Shape> _argShapes;
        private readonly Shape _shape;
        private readonly string _member;

        internal InvokeExpression(
            Receiver receiver,
            IReadOnlyList<string> path,
            IReadOnlyList<ValueExpression> args,
            IReadOnlyList<Shape> argShapes,
            Shape shape,
            string member)
        {
            _receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _argShapes = argShapes ?? throw new ArgumentNullException(nameof(argShapes));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
            _member = member ?? throw new ArgumentNullException(nameof(member));
        }

        internal override Shape OutputShape => _shape;

        internal override bool ReadsAwaited => _args.Any(arg => arg.ReadsAwaited);

        internal override Expr Write(Scope scope)
        {
            var (root, prefix) = _receiver.Locate(scope);
            var args = _args.Select(arg => arg.Write(scope)).ToList();
            return G.Call(root, prefix.Concat(_path).ToList(), args, _argShapes, _shape);
        }
    }

    /// <summary>
    /// What a method returned: bound once per block, by a <c>let</c> hoisted before the statement that
    /// first reads it, and read by name after. A promise is awaited by that <c>let</c> (grammar.md 3.1).
    /// </summary>
    internal sealed class MethodResultExpression : ValueExpression
    {
        private readonly ValueExpression _call;
        private readonly string _member;
        private readonly bool _awaits;

        internal MethodResultExpression(ValueExpression call, string member, bool awaits)
        {
            _call = call ?? throw new ArgumentNullException(nameof(call));
            _member = member ?? throw new ArgumentNullException(nameof(member));
            _awaits = awaits;
        }

        internal override Shape OutputShape => _call.OutputShape;

        internal override bool ReadsAwaited => _awaits || _call.ReadsAwaited;

        internal override Expr Write(Scope scope) =>
            scope.Result(this, JsonMemberName.Of(_member), _awaits, _member, () => _call.Write(scope));
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
            var values = DescribesArray(_shape) ? "queryAll" : "query";
            return G.Get(G.Get(G.Obj(scope.Objects.Document()), new[] { values }, Shape.None), new[] { _key }, _shape);
        }

        private static bool DescribesArray(Shape shape) =>
            shape.TryGetNullableInnerShape(out var inner) ? DescribesArray(inner) : shape.Kind == "array";
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

        internal override Expr Write(Scope scope) =>
            G.Call(G.Obj(scope.Objects.Model()), new[] { "boundValueOf" }, new Expr[] { G.Lit(_inputId) }, G.NoShapes, _shape);
    }

    internal sealed class ObjectExpression : ValueExpression
    {
        private readonly IReadOnlyDictionary<string, ValueExpression> _fields;
        private readonly Shape _shape;

        internal ObjectExpression(IReadOnlyDictionary<string, ValueExpression> fields, Shape shape)
        {
            _fields = fields;
            _shape = shape;
        }

        internal override Shape OutputShape => _shape;

        internal override bool ReadsAwaited => _fields.Values.Any(value => value.ReadsAwaited);

        internal override Expr Write(Scope scope) =>
            G.Record(_fields.Select(field => (RecordEntry)new FieldEntry(field.Key, field.Value.Write(scope))));
    }

    internal sealed class ArrayExpression : ValueExpression
    {
        internal ArrayExpression(IReadOnlyList<ValueExpression> items, Shape shape)
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

    /// <summary>
    /// An array operation, written as JavaScript's own method with a lambda. The source may wait for a
    /// promise; the lambda body runs once per item, where nothing waits.
    /// </summary>
    internal sealed class ArrayOperation : ValueExpression
    {
        private readonly string _op;
        private readonly ValueExpression _source;
        private readonly ConditionGraph? _predicate;
        private readonly ValueExpression? _projection;
        private readonly Shape _shape;

        internal ArrayOperation(
            string op,
            ValueExpression source,
            Shape shape,
            ConditionGraph? predicate = null,
            ValueExpression? projection = null)
        {
            _op = op;
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _shape = shape;
            _predicate = predicate;
            _projection = projection;
        }

        internal override Shape OutputShape => _shape;

        internal override bool ReadsAwaited => _source.ReadsAwaited;

        internal override Expr Write(Scope scope)
        {
            var items = G.Op(Operator.ToArray, _source.Write(scope));
            return scope.WithoutHoisting(() => Operate(items, scope));
        }

        private Expr Operate(Expr items, Scope scope)
        {
            switch (_op)
            {
                case "count":
                    return G.Get(items, new[] { "length" }, Shape.Number);
                case "filter":
                    return G.Call(items, new[] { "filter" }, new Expr[] { Predicate(scope) }, G.NoShapes, _shape);
                case "map":
                    return G.Call(items, new[] { "map" }, new Expr[] { Projection(scope) }, G.NoShapes, _shape);
                case "sum":
                    return Sum(items, scope);
                case "any":
                    return _predicate == null
                        ? G.Op(Operator.Gt, G.Get(items, new[] { "length" }, Shape.Number), G.Lit(0))
                        : G.Call(items, new[] { "some" }, new Expr[] { Predicate(scope) }, G.NoShapes, Shape.Boolean);
                case "all":
                    return G.Call(items, new[] { "every" }, new Expr[] { Predicate(scope) }, G.NoShapes, Shape.Boolean);
                case "find":
                    return Find(items, scope);
                case "orderBy":
                    return OrderBy(items, scope, descending: false);
                default:
                    return OrderBy(items, scope, descending: true);
            }
        }

        private FnExpr Predicate(Scope scope)
        {
            var predicate = _predicate ?? throw new InvalidOperationException("Array operation '" + _op + "' needs a predicate.");
            var item = scope.Fresh("x");
            return G.Fn(predicate.Write(scope.WithItem(item)), item);
        }

        private FnExpr Projection(Scope scope)
        {
            var projection = _projection ?? throw new InvalidOperationException("Array operation '" + _op + "' needs a selector.");
            var item = scope.Fresh("x");
            return G.Fn(projection.Write(scope.WithItem(item)), item);
        }

        // Sum adds each selected value as a finite number, so text never concatenates.
        private Expr Sum(Expr items, Scope scope)
        {
            var total = scope.Fresh("s");
            var item = scope.Fresh("x");
            var term = _projection == null ? (Expr)G.Ref(item) : _projection.Write(scope.WithItem(item));
            var add = G.Fn(G.Op(Operator.Add, G.Ref(total), G.Op(Operator.Finite, term)), total, item);
            return G.Call(items, new[] { "reduce" }, new Expr[] { add, G.Lit(0) }, G.NoShapes, Shape.Number);
        }

        // Find gives null when nothing matches, then applies the selector to what it found.
        private Expr Find(Expr items, Scope scope)
        {
            var found = G.Op(Operator.Coalesce, G.Call(items, new[] { "find" }, new Expr[] { Predicate(scope) }, G.NoShapes, Shape.None), G.Null);
            if (_projection == null) return G.As(found, _shape);
            var match = scope.Fresh("x");
            var selected = _projection.Write(scope.WithItem(match));
            var select = G.Fn(G.Op(Operator.Cond, G.Eq(G.Ref(match), G.Null), G.Null, selected), match);
            return G.As(G.Apply(select, found), _shape);
        }

        private Expr OrderBy(Expr items, Scope scope, bool descending)
        {
            var key = _projection ?? throw new InvalidOperationException("Array operation '" + _op + "' needs a key selector.");
            var left = scope.Fresh("a");
            var right = scope.Fresh("b");
            Expr compare = G.Op(Operator.Order, key.Write(scope.WithItem(left)), key.Write(scope.WithItem(right)));
            if (descending) compare = G.Op(Operator.Neg, compare);
            return G.Call(items, new[] { "toSorted" }, new Expr[] { G.Fn(compare, left, right) }, G.NoShapes, _shape);
        }
    }
}
