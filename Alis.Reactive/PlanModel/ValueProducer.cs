using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Base class for all value nodes in a reactive plan. Not constructed in application code.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueProducer>))]
    public abstract class ValueProducer
    {
        private protected ValueProducer() { }

        internal abstract Shape OutputShape { get; }

        internal static ValueProducer Literal(bool value) =>
            new LiteralProducer(value, Shape.Boolean);

        internal static ValueProducer Literal(string value) =>
            new LiteralProducer(value, Shape.String);

        internal static ValueProducer Literal(int value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(long value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(decimal value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(double value) =>
            new LiteralProducer(value, Shape.Number);

        internal static ValueProducer Literal(DateTime value) =>
            new LiteralProducer(value.ToString("O"), Shape.Date);

        internal static ValueProducer Null() =>
            new LiteralProducer(null, Shape.None);

        /// <summary>
        /// Creates a literal with any JSON-serializable value.
        /// System.Text.Json handles serialization at Render time.
        /// No reflection. No type inspection. The serializer does the work.
        /// </summary>
        internal static ValueProducer LiteralRaw(object? value, Shape shape) =>
            new LiteralProducer(value, shape);

        internal static ValueProducer LiteralFromValue(object? value)
        {
            if (value == null) return Null();

            return LiteralRaw(value, Shape.FromValue(value));
        }

        /// <summary>Creates a ReadProducer that reads a URL query parameter by name.
        /// Default shape is String because URL params are inherently strings.</summary>
        internal static ValueProducer ReadUrl(string paramName) =>
            Read(UrlSource.Instance, paramName, Shape.String);

        internal static ValueProducer ReadUrl(string paramName, Shape shape) =>
            Read(UrlSource.Instance, paramName, shape);

        internal static ValueProducer Read(Source from, string member) =>
            new ReadProducer(ValueRead.Property(from, member, Shape.None));

        internal static ValueProducer Read(Source from, string member, Path path) =>
            new ReadProducer(ValueRead.Property(from, member, path, Shape.None));

        internal static ValueProducer Read(Source from, string member, Shape shape) =>
            new ReadProducer(ValueRead.Property(from, member, shape));

        internal static ValueProducer Read(Source from, string member, Path path, Shape shape) =>
            new ReadProducer(ValueRead.Property(from, member, path, shape));

        internal static ValueProducer ReadPayload(PayloadSource from, string path) =>
            Read(from, path, Path.Parse(path));

        internal static ValueProducer ReadPayload(PayloadSource from, string path, Shape shape) =>
            Read(from, path, Path.Parse(path), shape);

        internal static ValueProducer Invoke(RuntimeObjectSource from, string method, Shape returns, IReadOnlyList<ValueProducer> args) =>
            new ReadProducer(ValueRead.Method(from, method, returns, ValueArguments.Of(args)));

        internal static ObjectProducer Object(IReadOnlyDictionary<string, ValueProducer> fields)
        {
            var objectFields = ValueObjectFields.From(fields);
            return new ObjectProducer(objectFields, objectFields.Shape);
        }

        internal static ObjectProducer Object(IReadOnlyDictionary<string, ValueProducer> fields, Shape shape) =>
            new ObjectProducer(ValueObjectFields.From(fields), shape);

        internal static ValueProducer Array(IReadOnlyList<ValueProducer> items)
        {
            var arrayItems = ValueArrayItems.From(items);
            return new ArrayProducer(arrayItems, arrayItems.Shape);
        }

        internal static ValueProducer Array(IReadOnlyList<ValueProducer> items, Shape shape) =>
            new ArrayProducer(ValueArrayItems.From(items), shape);
    }

    /// <summary>A constant value embedded in the plan.</summary>
    /// <remarks>
    /// Created when a literal is passed to a builder such as <c>p.Element("id").SetText("hello")</c>.
    /// </remarks>
    public sealed class LiteralProducer : ValueProducer
    {
        /// <summary>Gets the kind. Always <c>"literal"</c>.</summary>
        public string Kind => "literal";
        /// <summary>Gets the constant value embedded in the plan.</summary>
        [JsonInclude]
        public object? Value { get; }
        /// <summary>Gets the expected type shape. Defaults to <see cref="PlanModel.Shape.None"/> when not specified.</summary>
        public Shape Shape { get; }

        internal LiteralProducer(object? value, Shape shape)
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
    public sealed class ReadProducer : ValueProducer
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

        internal ReadProducer(ValueRead read)
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

        internal static ValueRead Method(RuntimeObjectSource from, string member, Shape shape, ValueArguments args) =>
            new ValueRead(
                ValueReadTarget.ForMember(from, member),
                shape,
                ValueReadAccess.Method(args));
    }

    internal sealed class ValueReadTarget
    {
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
        private const string WholePayloadMember = "responseBody";

        internal static Path FromMember(MemberName member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            if (string.Equals(member.Value, WholePayloadMember, StringComparison.Ordinal))
                return Path.None;

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

        internal static ValueReadAccess Method(ValueArguments args) =>
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
        private readonly ValueArguments _args;

        internal MethodValueReadAccess(ValueArguments args)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));
        }

        /// <summary>Gets the kind. Always <c>"method"</c>.</summary>
        public override string Kind => "method";

        /// <summary>Gets method arguments.</summary>
        public IReadOnlyList<ValueProducer> Args => _args.ItemsForJson;
    }

    internal sealed class ValueArguments
    {
        private readonly IReadOnlyList<ValueProducer> _items;

        private ValueArguments(IReadOnlyList<ValueProducer> items)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
        }

        internal static ValueArguments None { get; } =
            new ValueArguments(System.Array.Empty<ValueProducer>());

        internal IReadOnlyList<ValueProducer> ItemsForJson => _items;

        internal static ValueArguments Of(IReadOnlyList<ValueProducer> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                return None;

            var snapshot = new List<ValueProducer>();
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentException("Value argument must not be null.", nameof(items));

                snapshot.Add(item);
            }

            return new ValueArguments(snapshot);
        }
    }

    internal sealed class ValueObjectFields
    {
        private readonly IReadOnlyDictionary<string, ValueProducer> _fields;
        private readonly Shape _shape;

        private ValueObjectFields(IReadOnlyDictionary<string, ValueProducer> fields, Shape shape)
        {
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal IReadOnlyDictionary<string, ValueProducer> ForJson => _fields;
        internal Shape Shape => _shape;

        internal static ValueObjectFields From(IReadOnlyDictionary<string, ValueProducer> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));

            var snapshot = new Dictionary<string, ValueProducer>(StringComparer.Ordinal);
            var shapeFields = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var field in fields)
            {
                var fieldName = ValueObjectFieldName.Of(field.Key);
                var fieldValue = RequireFieldValue(field.Value, fieldName);
                snapshot[fieldName.Value] = fieldValue;
                shapeFields[fieldName.Value] = fieldValue.OutputShape;
            }

            return new ValueObjectFields(snapshot, Shape.ObjectOf(shapeFields));
        }

        private static ValueProducer RequireFieldValue(ValueProducer value, ValueObjectFieldName fieldName)
        {
            if (value == null)
                throw new ArgumentException(
                    "Object field '" + fieldName.Value + "' must have a value producer.",
                    nameof(value));

            return value;
        }
    }

    internal sealed class ValueObjectFieldName
    {
        private ValueObjectFieldName(string value)
        {
            Value = value;
        }

        internal string Value { get; }

        internal static ValueObjectFieldName Of(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Object field name must not be null or whitespace.", nameof(value));

            return new ValueObjectFieldName(value);
        }
    }

    internal sealed class ValueArrayItems
    {
        private readonly IReadOnlyList<ValueProducer> _items;
        private readonly Shape _shape;

        private ValueArrayItems(IReadOnlyList<ValueProducer> items, Shape shape)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            _shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal IReadOnlyList<ValueProducer> ForJson => _items;
        internal Shape Shape => _shape;

        internal static ValueArrayItems From(IReadOnlyList<ValueProducer> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                return Empty;

            var snapshot = new List<ValueProducer>();
            foreach (var item in items)
            {
                if (item == null)
                    throw new ArgumentException("Array item must not be null.", nameof(items));

                snapshot.Add(item);
            }

            return new ValueArrayItems(snapshot, ValueArrayOutputContract.From(snapshot).Shape);
        }

        private static ValueArrayItems Empty { get; } =
            new ValueArrayItems(
                System.Array.Empty<ValueProducer>(),
                ValueArrayOutputContract.Empty.Shape);
    }

    internal sealed class ValueArrayOutputContract
    {
        private ValueArrayOutputContract(Shape shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal Shape Shape { get; }

        internal static ValueArrayOutputContract Empty { get; } =
            new ValueArrayOutputContract(Shape.ArrayOf(Shape.Any));

        internal static ValueArrayOutputContract From(IReadOnlyList<ValueProducer> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));
            if (items.Count == 0)
                return Empty;

            var observedItemShape = ObservedArrayItemShape.From(items);
            if (!observedItemShape.HasSpecificShape)
                return Unconstrained;

            var itemShapeIsStable = ItemsShareShape(items, observedItemShape.SpecificShape);
            return itemShapeIsStable
                ? new ValueArrayOutputContract(Shape.ArrayOf(observedItemShape.SpecificShape))
                : Unconstrained;
        }

        private static ValueArrayOutputContract Unconstrained { get; } =
            new ValueArrayOutputContract(Shape.ArrayOf(Shape.Any));

        private static bool ItemsShareShape(IReadOnlyList<ValueProducer> items, Shape expected)
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

    internal sealed class ObservedArrayItemShape
    {
        private readonly Shape _specificShape;

        private ObservedArrayItemShape(Shape specificShape, bool hasSpecificShape)
        {
            _specificShape = specificShape ?? throw new ArgumentNullException(nameof(specificShape));
            HasSpecificShape = hasSpecificShape;
        }

        internal bool HasSpecificShape { get; }

        internal Shape SpecificShape
        {
            get
            {
                if (!HasSpecificShape)
                    throw new InvalidOperationException("No specific array item shape was observed.");

                return _specificShape;
            }
        }

        internal static ObservedArrayItemShape From(IReadOnlyList<ValueProducer> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            foreach (var item in items)
            {
                if (!item.OutputShape.IsNone)
                    return Specific(item.OutputShape);
            }

            return None;
        }

        private static ObservedArrayItemShape Specific(Shape shape) =>
            new ObservedArrayItemShape(shape, hasSpecificShape: true);

        private static ObservedArrayItemShape None { get; } =
            new ObservedArrayItemShape(Shape.Any, hasSpecificShape: false);
    }

    /// <summary>A composite value built from named field expressions.</summary>
    public sealed class ObjectProducer : ValueProducer
    {
        private readonly ValueObjectFields _fields;

        /// <summary>Gets the kind. Always <c>"object"</c>.</summary>
        public string Kind => "object";
        /// <summary>Gets the named fields and their value expressions.</summary>
        public IReadOnlyDictionary<string, ValueProducer> Fields => _fields.ForJson;
        /// <summary>Gets the expected type shape. Defaults to none when not specified.</summary>
        public Shape Shape { get; }

        internal ObjectProducer(ValueObjectFields fields, Shape shape)
        {
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }

    /// <summary>A composite value built from ordered item expressions.</summary>
    public sealed class ArrayProducer : ValueProducer
    {
        private readonly ValueArrayItems _items;

        /// <summary>Gets the kind. Always <c>"array"</c>.</summary>
        public string Kind => "array";
        /// <summary>Gets the ordered item expressions.</summary>
        public IReadOnlyList<ValueProducer> Items => _items.ForJson;
        /// <summary>Gets the expected type shape. Defaults to none when not specified.</summary>
        public Shape Shape { get; }

        internal ArrayProducer(ValueArrayItems items, Shape shape)
        {
            _items = items ?? throw new ArgumentNullException(nameof(items));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        internal override Shape OutputShape => Shape;
    }
}
