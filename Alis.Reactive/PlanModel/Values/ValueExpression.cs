using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Wire base for Reactive Plan value nodes authored through DSL value expressions.
    /// </summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<ValueExpression>))]
    public abstract class ValueExpression
    {
        private protected ValueExpression() { }

        internal abstract Shape OutputShape { get; }

        internal static ValueExpression Literal(bool value) =>
            new LiteralExpression(value, Shape.Boolean);

        internal static ValueExpression Literal(string value) =>
            new LiteralExpression(value, Shape.String);

        internal static ValueExpression Literal(int value) =>
            new LiteralExpression(value, Shape.Number);

        internal static ValueExpression Literal(long value) =>
            new LiteralExpression(value, Shape.Number);

        internal static ValueExpression Literal(decimal value) =>
            new LiteralExpression(value, Shape.Number);

        internal static ValueExpression Literal(double value) =>
            new LiteralExpression(value, Shape.Number);

        internal static ValueExpression Literal(DateTime value) =>
            new LiteralExpression(value.ToString("O"), Shape.Date);

        internal static ValueExpression Null() =>
            new LiteralExpression(null, Shape.None);

        /// <summary>Stores a literal value without inspecting it; callers provide the plan shape explicitly.</summary>
        internal static ValueExpression LiteralRaw(object? value, Shape shape) =>
            new LiteralExpression(value, shape);

        internal static ValueExpression LiteralFromValue(object? value)
        {
            if (value == null) return Null();

            return LiteralRaw(value, Shape.FromValue(value));
        }

        /// <summary>Reads a URL query parameter; untyped URL reads default to string shape.</summary>
        internal static ValueExpression ReadUrl(string paramName) =>
            Read(UrlSource.Instance, paramName, Shape.String);

        internal static ValueExpression ReadUrl(string paramName, Shape shape) =>
            Read(UrlSource.Instance, paramName, shape);

        /// <summary>
        /// Reads a DOM member by element ID. The path is carried so <c>RuntimePath</c>
        /// can traverse without a component contract.
        /// </summary>
        internal static ValueExpression ReadDom(string elementId, string member, Shape shape) =>
            Read(DomSource.Of(elementId), member, Path.Parse(member), shape);

        internal static ValueExpression Read(Source from, string member) =>
            new ReadExpression(ValueRead.Property(from, member, Shape.None));

        internal static ValueExpression Read(Source from, string member, Path path) =>
            new ReadExpression(ValueRead.Property(from, member, path, Shape.None));

        internal static ValueExpression Read(Source from, string member, Shape shape) =>
            new ReadExpression(ValueRead.Property(from, member, shape));

        internal static ValueExpression Read(Source from, string member, Path path, Shape shape) =>
            new ReadExpression(ValueRead.Property(from, member, path, shape));

        internal static ValueExpression ReadPayload(PayloadSource from, string path) =>
            Read(from, path, Path.Parse(path));

        internal static ValueExpression ReadPayload(PayloadSource from, string path, Shape shape) =>
            Read(from, path, Path.Parse(path), shape);

        internal static ValueExpression ReadWholePayload(PayloadSource from) =>
            new ReadExpression(ValueRead.WholePayload(from, Shape.None));

        internal static ValueExpression ReadWholePayload(PayloadSource from, Shape shape) =>
            new ReadExpression(ValueRead.WholePayload(from, shape));

        /// <summary>Reads the current array element itself (identity, <c>x =&gt; x</c>) for primitive-element arrays.</summary>
        internal static ValueExpression ReadWholeElement() =>
            new ReadExpression(ValueRead.WholeElement(PayloadSource.Element(), Shape.None));

        internal static ValueExpression ReadWholeElement(Shape shape) =>
            new ReadExpression(ValueRead.WholeElement(PayloadSource.Element(), shape));

        internal static ValueExpression Invoke(RuntimeObjectSource from, string method, Shape returns, IReadOnlyList<ValueExpression> args) =>
            new ReadExpression(ValueRead.Method(from, method, returns, args));

        /// <summary>
        /// Invokes a method on the current array element. The path carries receiver
        /// traversal plus method name so <c>RuntimePath.call</c> binds the correct owner.
        /// </summary>
        internal static ValueExpression InvokeElement(string receiverPath, string method, Shape returns, IReadOnlyList<ValueExpression> args)
        {
            var fullPath = string.IsNullOrEmpty(receiverPath) ? method : receiverPath + "." + method;
            return new ReadExpression(ValueRead.Method(PayloadSource.Element(), method, Path.Parse(fullPath), returns, args));
        }

        internal static ObjectExpression Object(IReadOnlyDictionary<string, ValueExpression> fields)
        {
            var objectFields = ObjectFields(fields);
            return new ObjectExpression(objectFields.Fields, objectFields.Shape);
        }

        internal static ObjectExpression Object(IReadOnlyDictionary<string, ValueExpression> fields, Shape shape)
        {
            var objectFields = ObjectFields(fields);
            return new ObjectExpression(objectFields.Fields, shape);
        }

        internal static ValueExpression Array(IReadOnlyList<ValueExpression> items)
        {
            var arrayItems = ArrayItems(items);
            return new ArrayExpression(arrayItems.Items, arrayItems.Shape);
        }

        internal static ValueExpression Array(IReadOnlyList<ValueExpression> items, Shape shape)
        {
            var arrayItems = ArrayItems(items);
            return new ArrayExpression(arrayItems.Items, shape);
        }

        internal static ValueExpression ArrayCount(ValueExpression source, Shape itemShape) =>
            new ArrayOperationExpression("count", source, itemShape, Shape.Number);

        internal static ValueExpression ArrayFilter(ValueExpression source, ConditionGraph predicate, Shape itemShape) =>
            new ArrayOperationExpression("filter", source, itemShape, Shape.ArrayOf(itemShape), predicate);

        internal static ValueExpression ArrayMap(ValueExpression source, ValueExpression projection, Shape itemShape, Shape resultItemShape) =>
            new ArrayOperationExpression("map", source, itemShape, Shape.ArrayOf(resultItemShape), predicate: null, projection: projection);

        /// <summary>Sums a selector result, or the numeric elements themselves when projection is null.</summary>
        internal static ValueExpression ArraySum(ValueExpression source, ValueExpression? projection, Shape itemShape) =>
            new ArrayOperationExpression("sum", source, itemShape, Shape.Number, predicate: null, projection: projection);

        /// <summary>Null predicate means non-empty; otherwise evaluate against each element.</summary>
        internal static ValueExpression ArrayAny(ValueExpression source, ConditionGraph? predicate, Shape itemShape) =>
            new ArrayOperationExpression("any", source, itemShape, Shape.Boolean, predicate: predicate);

        internal static ValueExpression ArrayAll(ValueExpression source, ConditionGraph predicate, Shape itemShape) =>
            new ArrayOperationExpression("all", source, itemShape, Shape.Boolean, predicate: predicate);

        /// <summary>Finds the first matching element or projection; runtime returns null when none match.</summary>
        internal static ValueExpression ArrayFind(
            ValueExpression source, ConditionGraph? predicate, ValueExpression? projection, Shape itemShape, Shape resultShape) =>
            new ArrayOperationExpression("find", source, itemShape, resultShape, predicate: predicate, projection: projection);

        internal static ValueExpression ArrayOrderBy(ValueExpression source, ValueExpression key, Shape itemShape, bool descending) =>
            new ArrayOperationExpression(
                descending ? "orderByDescending" : "orderBy", source, itemShape, Shape.ArrayOf(itemShape), predicate: null, projection: key);

        private static (IReadOnlyDictionary<string, ValueExpression> Fields, Shape Shape) ObjectFields(
            IReadOnlyDictionary<string, ValueExpression> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var snapshot = new Dictionary<string, ValueExpression>(StringComparer.Ordinal);
            var shapeFields = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                var fieldName = ObjectFieldName(field.Key);
                var fieldValue = ObjectFieldValue(field.Value, fieldName);
                snapshot[fieldName] = fieldValue;
                shapeFields[fieldName] = fieldValue.OutputShape;
            }

            return (snapshot, Shape.ObjectOf(shapeFields));
        }

        private static string ObjectFieldName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Object field name must not be null or whitespace.", nameof(value));

            return value;
        }

        private static ValueExpression ObjectFieldValue(ValueExpression value, string fieldName)
        {
            if (value == null)
                throw new ArgumentException(
                    "Object field '" + fieldName + "' must have a value expression.",
                    nameof(value));

            return value;
        }

        private static (IReadOnlyList<ValueExpression> Items, Shape Shape) ArrayItems(
            IReadOnlyList<ValueExpression> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                return (System.Array.Empty<ValueExpression>(), Shape.ArrayOf(Shape.Any));

            var snapshot = new List<ValueExpression>();
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentException("Array item must not be null.", nameof(items));

                snapshot.Add(item);
            }

            return (snapshot, ArrayShape(snapshot));
        }

        private static Shape ArrayShape(IReadOnlyList<ValueExpression> items)
        {
            if (!TryFindSpecificArrayItemShape(items, out var itemShape))
                return Shape.ArrayOf(Shape.Any);

            return ArrayItemsShareShape(items, itemShape)
                ? Shape.ArrayOf(itemShape)
                : Shape.ArrayOf(Shape.Any);
        }

        private static bool TryFindSpecificArrayItemShape(
            IReadOnlyList<ValueExpression> items,
            [NotNullWhen(true)] out Shape? shape)
        {
            foreach (var item in items)
            {
                if (item.OutputShape.IsNone) continue;

                shape = item.OutputShape;
                return true;
            }

            shape = null;
            return false;
        }

        private static bool ArrayItemsShareShape(IReadOnlyList<ValueExpression> items, Shape expected)
        {
            foreach (var item in items)
            {
                var itemShape = item.OutputShape;
                if (itemShape.IsNone)
                    return false;
                if (!itemShape.Equals(expected))
                    return false;
            }

            return true;
        }
    }

    /// <summary>Literal value node serialized directly into generated plan JSON.</summary>
    /// <remarks>
    /// Created when a literal is passed to a builder such as <c>p.Element("id").SetText("hello")</c>.
    /// </remarks>
    public sealed class LiteralExpression : ValueExpression
    {
        /// <summary>Wire discriminator for literal value nodes. Always <c>"literal"</c>.</summary>
        public string Kind => "literal";
        /// <summary>Serialized literal payload; may be <see langword="null"/> for explicit null values.</summary>
        [JsonInclude]
        public object? Value { get; }
        /// <summary>Output shape declared by the authoring layer, or <see cref="PlanModel.Shape.None"/> when unspecified.</summary>
        public Shape Shape { get; }

        internal LiteralExpression(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }

    /// <summary>Value-read node resolved from a source when the Reactive Plan executes.</summary>
    /// <remarks>
    /// Created by source-reading builders such as <c>p.Plugin&lt;int&gt;("array", "count").Arg(json, x =&gt; x.Items)</c>.
    /// </remarks>
    public sealed class ReadExpression : ValueExpression
    {
        private readonly ValueRead _read;

        /// <summary>Wire discriminator for value read nodes. Always <c>"read"</c>.</summary>
        public string Kind => "read";
        /// <summary>
        /// Source object or payload scope for this read, such as a component, plugin, or event payload.
        /// </summary>
        public Source From => _read.From;
        /// <summary>Plan member name resolved against the value source.</summary>
        public string Member => _read.Member.Value;
        /// <summary>Nested runtime traversal path, or empty for direct member reads.</summary>
        public Path Path => _read.Path;
        /// <summary>Output shape declared by the authoring layer, or <see cref="PlanModel.Shape.None"/> when unspecified.</summary>
        public Shape Shape => _read.Shape;
        /// <summary>Access contract that tells the runtime whether to read a property or invoke a method.</summary>
        public ValueReadAccess Access => _read.Access;

        internal ReadExpression(ValueRead read)
        {
            _read = read ?? throw new ArgumentNullException(nameof(read));
        }

        internal override Shape OutputShape => Shape;
    }

    internal sealed class ValueRead
    {
        private ValueRead(ValueReadTarget target, Shape shape, ValueReadAccess access)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            Access = access ?? throw new ArgumentNullException(nameof(access));
        }

        internal Source From => Target.From;
        internal MemberName Member => Target.Member;
        internal Path Path => Target.Path;
        internal Shape Shape { get; }
        internal ValueReadAccess Access { get; }

        private ValueReadTarget Target { get; }

        internal static ValueRead Property(Source from, string member, Shape shape) =>
            new ValueRead(
                ValueReadTarget.ForMember(from, member),
                shape,
                ValueReadAccess.Property);

        internal static ValueRead Property(Source from, string member, Path path, Shape shape) =>
            new ValueRead(
                ValueReadTarget.ForMember(from, member, path),
                shape,
                ValueReadAccess.Property);

        internal static ValueRead Method(RuntimeObjectSource from, string member, Shape shape, IReadOnlyList<ValueExpression> args) =>
            new ValueRead(
                ValueReadTarget.ForMember(from, member),
                shape,
                ValueReadAccess.Method(args));

        internal static ValueRead Method(Source from, string member, Path path, Shape shape, IReadOnlyList<ValueExpression> args) =>
            new ValueRead(
                ValueReadTarget.ForMember(from, member, path),
                shape,
                ValueReadAccess.Method(args));

        internal static ValueRead WholePayload(PayloadSource from, Shape shape) =>
            new ValueRead(
                ValueReadTarget.ForWholePayload(from),
                shape,
                ValueReadAccess.Property);

        internal static ValueRead WholeElement(PayloadSource from, Shape shape) =>
            new ValueRead(
                ValueReadTarget.ForWholeElement(from),
                shape,
                ValueReadAccess.Property);
    }

    internal sealed class ValueReadTarget
    {
        private const string WholePayloadMember = "responseBody";
        private const string WholeElementMember = "elementValue";

        private ValueReadTarget(Source from, MemberName member, Path path)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        internal Source From { get; }
        internal MemberName Member { get; }
        internal Path Path { get; }

        internal static ValueReadTarget ForMember(Source from, string member) =>
            ForMember(from, MemberName.Of(member));

        internal static ValueReadTarget ForMember(Source from, string member, Path path) =>
            new ValueReadTarget(from, MemberName.Of(member), path);

        internal static ValueReadTarget ForWholePayload(PayloadSource from) =>
            new ValueReadTarget(from, MemberName.Of(WholePayloadMember), Path.None);

        internal static ValueReadTarget ForWholeElement(PayloadSource from) =>
            new ValueReadTarget(from, MemberName.Of(WholeElementMember), Path.None);

        private static ValueReadTarget ForMember(Source from, MemberName member) =>
            new ValueReadTarget(from, member, ValueReadPath.For(from, member));
    }

    internal static class ValueReadPath
    {
        internal static Path For(Source from, MemberName member)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (member == null) throw new ArgumentNullException(nameof(member));

            if (from is PayloadSource) return PayloadReadPath.FromMember(member);

            return Path.None;
        }
    }

    internal static class PayloadReadPath
    {
        internal static Path FromMember(MemberName member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            return Path.Parse(member.Value);
        }
    }

    /// <summary>Base wire contract for how a value read accesses its target member.</summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<ValueReadAccess>))]
    public abstract class ValueReadAccess
    {
        private protected ValueReadAccess() { }

        internal static ValueReadAccess Property { get; } =
            new PropertyValueReadAccess();

        internal static ValueReadAccess Method(IReadOnlyList<ValueExpression> args) =>
            new MethodValueReadAccess(args);

        /// <summary>Wire discriminator for the read access shape.</summary>
        public abstract string Kind { get; }
    }

    /// <summary>Access contract for reading a source member as a property.</summary>
    public sealed class PropertyValueReadAccess : ValueReadAccess
    {
        /// <summary>Wire discriminator for property reads. Always <c>"property"</c>.</summary>
        public override string Kind => "property";
    }

    /// <summary>Access contract for invoking a source member as a method.</summary>
    public sealed class MethodValueReadAccess : ValueReadAccess
    {
        private readonly IReadOnlyList<ValueExpression> _args;

        internal MethodValueReadAccess(IReadOnlyList<ValueExpression> args)
        {
            _args = OrderedArguments(args);
        }

        /// <summary>Wire discriminator for method reads. Always <c>"method"</c>.</summary>
        public override string Kind => "method";

        /// <summary>Argument value expressions evaluated in call order before invocation.</summary>
        public IReadOnlyList<ValueExpression> Args => _args;

        private static IReadOnlyList<ValueExpression> OrderedArguments(IReadOnlyList<ValueExpression> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                return Array.Empty<ValueExpression>();

            var snapshot = new List<ValueExpression>();
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentException("Value argument must not be null.", nameof(items));

                snapshot.Add(item);
            }

            return snapshot;
        }
    }

    /// <summary>Object value node assembled from named field expressions.</summary>
    public sealed class ObjectExpression : ValueExpression
    {
        private readonly IReadOnlyDictionary<string, ValueExpression> _fields;

        /// <summary>Wire discriminator for object value nodes. Always <c>"object"</c>.</summary>
        public string Kind => "object";
        /// <summary>Field map evaluated by name when the runtime builds the object value.</summary>
        public IReadOnlyDictionary<string, ValueExpression> Fields => _fields;
        /// <summary>Output shape declared by the authoring layer, or <see cref="PlanModel.Shape.None"/> when unspecified.</summary>
        public Shape Shape { get; }

        internal ObjectExpression(IReadOnlyDictionary<string, ValueExpression> fields, Shape shape)
        {
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }

    /// <summary>Array value node assembled from ordered item expressions.</summary>
    public sealed class ArrayExpression : ValueExpression
    {
        private readonly IReadOnlyList<ValueExpression> _items;

        /// <summary>Wire discriminator for array value nodes. Always <c>"array"</c>.</summary>
        public string Kind => "array";
        /// <summary>Item expressions evaluated in order when the runtime builds the array value.</summary>
        public IReadOnlyList<ValueExpression> Items => _items;
        /// <summary>Output shape declared by the authoring layer, or <see cref="PlanModel.Shape.None"/> when unspecified.</summary>
        public Shape Shape { get; }

        internal ArrayExpression(IReadOnlyList<ValueExpression> items, Shape shape)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }

    /// <summary>Deterministic operation over the elements of an array-shaped value.</summary>
    /// <remarks>
    /// One node with an <c>Op</c> sub-discriminator. The runtime normalizes
    /// array-like or iterable source values at the input boundary, then produces
    /// the declared output. Predicate and projection are present only for
    /// operations that need them.
    /// </remarks>
    public sealed class ArrayOperationExpression : ValueExpression
    {
        /// <summary>Wire discriminator for array operation nodes. Always <c>"array-op"</c>.</summary>
        public string Kind => "array-op";
        /// <summary>Operation name dispatched by the runtime, such as <c>count</c> or <c>filter</c>.</summary>
        public string Op { get; }
        /// <summary>Value expression evaluated before the runtime normalizes the source to an array.</summary>
        public ValueExpression Source { get; }
        /// <summary>Per-element predicate, or null when the operation does not use one.</summary>
        /// <remarks>
        /// Nullable by design: count, map, sum, and ordering do not use a
        /// predicate; any may omit it for non-empty checks; filter, all, and find
        /// require one. Absence is modeled as null and omitted from JSON instead
        /// of using an "always true" sentinel, because "no predicate" and
        /// "match all" are different plan states. Predicates use the sync
        /// condition subset, never confirm, so per-element evaluation stays on
        /// the immediate lane.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ConditionGraph? Predicate { get; }
        /// <summary>Per-element projection, or <see langword="null"/> when the operation does not use one.</summary>
        /// <remarks>
        /// Nullable by design: count, filter, any, and all omit projection; map,
        /// sum, and ordering require a selector; find includes one only for field
        /// projection. It is evaluated against the element scope.
        /// </remarks>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ValueExpression? Projection { get; }
        /// <summary>Element shape expected from the source array.</summary>
        public Shape ItemShape { get; }
        /// <summary>Output shape produced by the operation.</summary>
        public Shape Shape { get; }

        internal ArrayOperationExpression(
            string op,
            ValueExpression source,
            Shape itemShape,
            Shape shape,
            ConditionGraph? predicate = null,
            ValueExpression? projection = null)
        {
            Op = op ?? throw new ArgumentNullException(nameof(op));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            ItemShape = itemShape ?? throw new ArgumentNullException(nameof(itemShape));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            Predicate = predicate;
            Projection = projection;
        }

        internal override Shape OutputShape => Shape;
    }
}
