using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// Creates a literal for a scalar value. For non-scalars, delegates to
        /// FromValue which produces the correct ValueProducer kind (Array, Object).
        /// </summary>
        internal static ValueProducer LiteralRaw(object value, Shape shape)
        {
            if (value == null) return new LiteralProducer(null, shape);
            if (IsScalar(value)) return new LiteralProducer(value, shape);
            // Non-scalar: build proper ValueProducer structure
            return FromValue(value, shape);
        }

        /// <summary>
        /// Converts any CLR value to the correct ValueProducer kind:
        /// scalar → LiteralProducer, array → ArrayProducer, object → ObjectProducer.
        /// </summary>
        internal static ValueProducer FromValue(object value, Shape shape)
        {
            if (value == null) return new LiteralProducer(null, shape);
            if (IsScalar(value)) return new LiteralProducer(value, shape);

            if (value is IEnumerable enumerable && !(value is string))
            {
                var items = new List<ValueProducer>();
                var itemShape = shape.Kind == "array" && shape.Item != null ? shape.Item : Shape.Any;
                foreach (var item in enumerable)
                    items.Add(FromValue(item, itemShape));
                return new ArrayProducer(items, shape);
            }

            // Object: reflect properties
            var fields = new Dictionary<string, ValueProducer>();
            foreach (var prop in value.GetType().GetProperties(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!prop.CanRead) continue;
                var val = prop.GetValue(value);
                var propShape = Shape.FromClrType(prop.PropertyType);
                fields[prop.Name] = FromValue(val, propShape);
            }
            return new ObjectProducer(fields, shape);
        }

        private static bool IsScalar(object value) =>
            value is string || value is bool ||
            value is int || value is long || value is decimal || value is double || value is float ||
            value is short || value is byte || value is sbyte || value is ushort || value is uint || value is ulong ||
            value is DateTime;

        internal static ValueProducer Read(Source from, string member, Path path = null, Shape shape = null) =>
            new ReadProducer(from, member, path, shape);

        internal static ValueProducer Object(Dictionary<string, ValueProducer> fields, Shape shape = null) =>
            new ObjectProducer(fields, shape);

        internal static ValueProducer Array(List<ValueProducer> items, Shape shape = null) =>
            new ArrayProducer(items, shape);

        /// <summary>
        /// Converts a POCO payload into an ObjectProducer by reflecting properties.
        /// Handles nested objects and arrays recursively via FromValue.
        /// </summary>
        internal static ValueProducer FromPayload<T>(T payload) =>
            FromValue(payload, Shape.Any);
    }

    public sealed class LiteralProducer : ValueProducer
    {
        public string Kind => "literal";
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
        public Dictionary<string, ValueProducer> Fields { get; }
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
        public List<ValueProducer> Items { get; }
        public Shape Shape { get; }

        internal ArrayProducer(List<ValueProducer> items, Shape shape)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Shape = shape == null || shape.IsNone ? null : shape;
        }
    }
}
