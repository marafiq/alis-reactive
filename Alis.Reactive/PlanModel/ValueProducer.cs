using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<ValueProducer>))]
    public abstract class ValueProducer
    {
        private protected ValueProducer() { }

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
        internal static ValueProducer LiteralRaw(object value, Shape shape) =>
            new LiteralProducer(value, shape);

        /// <summary>Creates a ReadProducer that reads a URL query parameter by name.
        /// Default shape is String because URL params are inherently strings.</summary>
        internal static ValueProducer ReadUrl(string paramName, Shape shape = null) =>
            Read(UrlSource.Instance, paramName, shape: shape ?? Shape.String);

        internal static ValueProducer Read(Source from, string member, Path path = null, Shape shape = null) =>
            new ReadProducer(from, member, path, shape);

        internal static ValueProducer Object(Dictionary<string, ValueProducer> fields, Shape shape = null) =>
            new ObjectProducer(fields, shape);

        internal static ValueProducer Array(List<ValueProducer> items, Shape shape = null) =>
            new ArrayProducer(items, shape);
    }

    public sealed class LiteralProducer : ValueProducer
    {
        public string Kind => "literal";
        [JsonInclude]
        public object Value { get; }
        public Shape Shape { get; }

        internal LiteralProducer(object value, Shape shape)
        {
            Value = value;
            Shape = shape == Shape.None ? null : shape;
        }
    }

    public sealed class ReadProducer : ValueProducer
    {
        public string Kind => "read";
        public Source From { get; }
        public string Member { get; }
        public Path Path { get; }
        public Shape Shape { get; }

        internal ReadProducer(Source from, string member, Path path, Shape shape)
        {
            From = from ?? throw new ArgumentNullException(nameof(from));
            Member = member ?? throw new ArgumentNullException(nameof(member));
            Path = path == null || path.IsNone ? null : path;
            Shape = shape == null || shape.IsNone ? null : shape;
        }
    }

    public sealed class ObjectProducer : ValueProducer
    {
        public string Kind => "object";
        public IReadOnlyDictionary<string, ValueProducer> Fields { get; }
        public Shape Shape { get; }

        internal ObjectProducer(Dictionary<string, ValueProducer> fields, Shape shape)
        {
            Fields = fields ?? throw new ArgumentNullException(nameof(fields));
            Shape = shape == null || shape.IsNone ? null : shape;
        }
    }

    public sealed class ArrayProducer : ValueProducer
    {
        public string Kind => "array";
        public IReadOnlyList<ValueProducer> Items { get; }
        public Shape Shape { get; }

        internal ArrayProducer(List<ValueProducer> items, Shape shape)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Shape = shape == null || shape.IsNone ? null : shape;
        }
    }
}
