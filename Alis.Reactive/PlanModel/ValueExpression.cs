using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for all value nodes in a reactive plan. Not constructed in application code.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueExpression>))]
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

        /// <summary>
        /// Creates a literal with any JSON-serializable value.
        /// System.Text.Json handles serialization at Render time.
        /// No reflection. No type inspection. The serializer does the work.
        /// </summary>
        internal static ValueExpression LiteralRaw(object? value, Shape shape) =>
            new LiteralExpression(value, shape);

        internal static ValueExpression LiteralFromValue(object? value)
        {
            if (value == null) return Null();

            return LiteralRaw(value, Shape.FromValue(value));
        }

        /// <summary>Creates a ReadExpression that reads a URL query parameter by name.
        /// Default shape is String because URL params are inherently strings.</summary>
        internal static ValueExpression ReadUrl(string paramName) =>
            Read(UrlSource.Instance, paramName, Shape.String);

        internal static ValueExpression ReadUrl(string paramName, Shape shape) =>
            Read(UrlSource.Instance, paramName, shape);

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

        internal static ValueExpression Invoke(RuntimeObjectSource from, string method, Shape returns, IReadOnlyList<ValueExpression> args) =>
            new ReadExpression(ValueRead.Method(from, method, returns, args));

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

    /// <summary>A constant value embedded in the plan.</summary>
    /// <remarks>
    /// Created when a literal is passed to a builder such as <c>p.Element("id").SetText("hello")</c>.
    /// </remarks>
    public sealed class LiteralExpression : ValueExpression
    {
        /// <summary>Gets the kind. Always <c>"literal"</c>.</summary>
        public string Kind => "literal";
        /// <summary>Gets the constant value embedded in the plan.</summary>
        [JsonInclude]
        public object? Value { get; }
        /// <summary>Gets the expected type shape. Defaults to <see cref="PlanModel.Shape.None"/> when not specified.</summary>
        public Shape Shape { get; }

        internal LiteralExpression(object? value, Shape shape)
        {
            Value = value;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }

    /// <summary>A value read from a live source when the plan executes in the browser.</summary>
    /// <remarks>
    /// Created by source-reading builders such as <c>p.Plugin&lt;int&gt;("array", "count").Arg(json, x =&gt; x.Items)</c>.
    /// </remarks>
    public sealed class ReadExpression : ValueExpression
    {
        private readonly ValueRead _read;

        /// <summary>Gets the kind. Always <c>"read"</c>.</summary>
        public string Kind => "read";
        /// <summary>
        /// Gets the value source: <see cref="ComponentSource"/>, <see cref="PluginSource"/>,
        /// <see cref="UrlSource"/>, or <see cref="PayloadSource"/>.
        /// </summary>
        public Source From => _read.From;
        /// <summary>Gets the property or method name to read on the source.</summary>
        public string Member => _read.Member.Value;
        /// <summary>Gets the nested property path. Defaults to empty for direct reads.</summary>
        public Path Path => _read.Path;
        /// <summary>Gets the expected type shape. Defaults to none when not specified.</summary>
        public Shape Shape => _read.Shape;
        /// <summary>Gets whether the read accesses a property or invokes a method.</summary>
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

        internal static ValueRead WholePayload(PayloadSource from, Shape shape) =>
            new ValueRead(
                ValueReadTarget.ForWholePayload(from),
                shape,
                ValueReadAccess.Property);
    }

    internal sealed class ValueReadTarget
    {
        private const string WholePayloadMember = "responseBody";

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

    /// <summary>Base class for the member access intent of a value read.</summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueReadAccess>))]
    public abstract class ValueReadAccess
    {
        private protected ValueReadAccess() { }

        internal static ValueReadAccess Property { get; } =
            new PropertyValueReadAccess();

        internal static ValueReadAccess Method(IReadOnlyList<ValueExpression> args) =>
            new MethodValueReadAccess(args);

        /// <summary>Gets the access kind.</summary>
        public abstract string Kind { get; }
    }

    /// <summary>Reads a property value from the source.</summary>
    public sealed class PropertyValueReadAccess : ValueReadAccess
    {
        /// <summary>Gets the kind. Always <c>"property"</c>.</summary>
        public override string Kind => "property";
    }

    /// <summary>Invokes a method and uses the returned value.</summary>
    public sealed class MethodValueReadAccess : ValueReadAccess
    {
        private readonly IReadOnlyList<ValueExpression> _args;

        internal MethodValueReadAccess(IReadOnlyList<ValueExpression> args)
        {
            _args = OrderedArguments(args);
        }

        /// <summary>Gets the kind. Always <c>"method"</c>.</summary>
        public override string Kind => "method";

        /// <summary>Gets method arguments.</summary>
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

    /// <summary>A composite value built from named field expressions.</summary>
    public sealed class ObjectExpression : ValueExpression
    {
        private readonly IReadOnlyDictionary<string, ValueExpression> _fields;

        /// <summary>Gets the kind. Always <c>"object"</c>.</summary>
        public string Kind => "object";
        /// <summary>Gets the named fields and their value expressions.</summary>
        public IReadOnlyDictionary<string, ValueExpression> Fields => _fields;
        /// <summary>Gets the expected type shape. Defaults to none when not specified.</summary>
        public Shape Shape { get; }

        internal ObjectExpression(IReadOnlyDictionary<string, ValueExpression> fields, Shape shape)
        {
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }

    /// <summary>A composite value built from ordered item expressions.</summary>
    public sealed class ArrayExpression : ValueExpression
    {
        private readonly IReadOnlyList<ValueExpression> _items;

        /// <summary>Gets the kind. Always <c>"array"</c>.</summary>
        public string Kind => "array";
        /// <summary>Gets the ordered item expressions.</summary>
        public IReadOnlyList<ValueExpression> Items => _items;
        /// <summary>Gets the expected type shape. Defaults to none when not specified.</summary>
        public Shape Shape { get; }

        internal ArrayExpression(IReadOnlyList<ValueExpression> items, Shape shape)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }
}
