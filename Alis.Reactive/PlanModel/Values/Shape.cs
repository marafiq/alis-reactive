using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class ShapeJsonConverter : JsonConverter<Shape>
    {
        public override void Write(Utf8JsonWriter writer, Shape value, JsonSerializerOptions options) =>
            value.WriteTo(writer);

        public override Shape Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }

    /// <summary>
    /// The type of a value in the plan, as MVC binds it: a scalar (string, number, boolean, date, instant),
    /// an array of a shape, a nullable shape, an object of named shapes, any value, or none.
    /// </summary>
    [JsonConverter(typeof(ShapeJsonConverter))]
    internal sealed class Shape : IEquatable<Shape>
    {
        // Declared before the shapes below, which are built with it.
        private static readonly IReadOnlyDictionary<string, Shape> NoFields =
            new ReadOnlyDictionary<string, Shape>(new Dictionary<string, Shape>());

        internal static readonly Shape String = new Shape("string");
        internal static readonly Shape Number = new Shape("number");
        internal static readonly Shape Boolean = new Shape("boolean");
        internal static readonly Shape Date = new Shape("date");
        /// <summary>A moment in time with its offset (<see cref="DateTimeOffset"/>), sent as the exact instant.</summary>
        internal static readonly Shape Instant = new Shape("instant");
        internal static readonly Shape Any = new Shape("any");
        internal static readonly Shape None = new Shape("none");

        // The item of an array or the inner shape of a nullable: one shape for those two kinds, none for
        // the others. A list, not a None sentinel, because None is itself built by these constructors.
        private readonly IReadOnlyList<Shape> _wrapped;
        private readonly IReadOnlyDictionary<string, Shape> _fields;

        private Shape(string kind)
            : this(kind, Array.Empty<Shape>(), NoFields)
        {
        }

        private Shape(string kind, IReadOnlyList<Shape> wrapped, IReadOnlyDictionary<string, Shape> fields)
        {
            Kind = kind;
            _wrapped = wrapped;
            _fields = fields;
        }

        /// <summary>JSON shape kind, such as <c>string</c>, <c>array</c>, <c>object</c>, or <c>nullable</c>.</summary>
        public string Kind { get; }

        internal bool IsNone => Kind == "none";

        /// <summary>
        /// True when a value of this shape serializes to one string, as an HTTP header, a route parameter or a
        /// query value needs: string, number, boolean, date, instant, or a nullable of one of them.
        /// </summary>
        internal bool IsScalar
        {
            get
            {
                switch (Kind)
                {
                    case "string":
                    case "number":
                    case "boolean":
                    case "date":
                    case "instant":
                        return true;
                    case "nullable":
                        return _wrapped[0].IsScalar;
                    default:
                        return false;
                }
            }
        }

        internal static Shape ArrayOf(Shape item) =>
            new Shape("array", new[] { Wrappable(item, nameof(item)) }, NoFields);

        internal static Shape Nullable(Shape inner) =>
            new Shape("nullable", new[] { Wrappable(inner, nameof(inner)) }, NoFields);

        internal static Shape ObjectOf(Dictionary<string, Shape> fields)
        {
            if (fields is null) throw new ArgumentNullException(nameof(fields));
            return new Shape("object", Array.Empty<Shape>(), new ReadOnlyDictionary<string, Shape>(fields));
        }

        private static Shape Wrappable(Shape shape, string parameter)
        {
            if (shape is null || shape.IsNone)
                throw new ArgumentException($"The {parameter} shape is required.", parameter);
            return shape;
        }

        internal bool TryGetArrayItemShape(out Shape item) => TryGetWrapped("array", out item);

        internal bool TryGetNullableInnerShape(out Shape inner) => TryGetWrapped("nullable", out inner);

        private bool TryGetWrapped(string kind, out Shape wrapped)
        {
            var wraps = Kind == kind;
            wrapped = wraps ? _wrapped[0] : None;
            return wraps;
        }

        internal string DescribeContract()
        {
            switch (Kind)
            {
                case "array":
                    return "array<" + _wrapped[0].DescribeContract() + ">";
                case "nullable":
                    return "nullable<" + _wrapped[0].DescribeContract() + ">";
                case "object":
                    return "object{" + string.Join(", ", _fields.Select(field => field.Key + ":" + field.Value.DescribeContract())) + "}";
                default:
                    return Kind;
            }
        }

        internal void WriteTo(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", Kind);
            if (Kind == "array") WriteMember(writer, "item", _wrapped[0]);
            if (Kind == "nullable") WriteMember(writer, "inner", _wrapped[0]);
            if (Kind == "object")
            {
                writer.WriteStartObject("fields");
                foreach (var field in _fields)
                    WriteMember(writer, field.Key, field.Value);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }

        private static void WriteMember(Utf8JsonWriter writer, string name, Shape shape)
        {
            writer.WritePropertyName(name);
            shape.WriteTo(writer);
        }

        // ── from CLR types ───────────────────────────────────────────────────────

        internal static Shape FromClrType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return Nullable(FromClrType(underlying));

            if (type == typeof(string)) return String;
            if (type == typeof(bool)) return Boolean;
            if (type == typeof(DateTimeOffset)) return Instant;
            if (IsDateType(type)) return Date;
            if (IsNumericType(type)) return Number;
            if (IsStringSerializedType(type)) return String;
            if (type.IsEnum) return String;

            if (TryGetCollectionItemShape(type, out var itemShape))
                return ArrayOf(itemShape);

            return Any;
        }

        internal static Shape CollectionItemShapeOrNone(Type type) =>
            TryGetCollectionItemShape(type, out var itemShape) ? itemShape : None;

        internal static Shape FromValue(object? value) =>
            value == null ? None : FromClrType(value.GetType());

        private static bool IsDateType(Type type)
            => type == typeof(DateTime)
#if NET6_0_OR_GREATER
               || type == typeof(DateOnly)
#endif
               ;

        private static bool IsNumericType(Type type)
            => type == typeof(byte) || type == typeof(sbyte) ||
               type == typeof(short) || type == typeof(ushort) ||
               type == typeof(int) || type == typeof(uint) ||
               type == typeof(long) || type == typeof(ulong) ||
               type == typeof(float) || type == typeof(double) || type == typeof(decimal);

        private static bool IsStringSerializedType(Type type)
            => type == typeof(Guid) || type == typeof(TimeSpan) || typeof(Field).IsAssignableFrom(type)
#if NET6_0_OR_GREATER
               || type == typeof(TimeOnly)
#endif
               ;

        // Text and dictionaries enumerate, but MVC binds neither as a list.
        private static bool TryGetCollectionItemShape(Type type, out Shape itemShape)
        {
            itemShape = None;
            var bindsAsList = type != typeof(string) && !IsDictionaryLike(type);
            if (!bindsAsList || !TryGetItemType(type, out var itemType)) return false;

            itemShape = FromClrType(itemType);
            return true;
        }

        private static bool TryGetItemType(Type type, out Type itemType)
        {
            if (type.IsArray && type.GetElementType() is Type elementType)
            {
                itemType = elementType;
                return true;
            }

            var itemTypes = (IsSupportedGenericCollection(type) ? new[] { type } : type.GetInterfaces())
                .Where(IsSupportedGenericCollection)
                .Select(collection => collection.GetGenericArguments()[0])
                .Distinct()
                .ToArray();
            itemType = itemTypes.Length == 1 ? itemTypes[0] : typeof(object);
            return itemTypes.Length == 1;
        }

        private static bool IsSupportedGenericCollection(Type type)
        {
            if (!type.IsGenericType) return false;

            var definition = type.GetGenericTypeDefinition();
            return definition == typeof(IEnumerable<>)
                   || definition == typeof(IReadOnlyCollection<>)
                   || definition == typeof(IReadOnlyList<>)
                   || definition == typeof(ICollection<>)
                   || definition == typeof(IList<>)
                   || definition == typeof(List<>)
                   || definition == typeof(HashSet<>)
                   || definition == typeof(ISet<>);
        }

        private static bool IsDictionaryLike(Type type) =>
            new[] { type }.Concat(type.GetInterfaces())
                .Any(candidate => candidate.IsGenericType && IsDictionaryDefinition(candidate.GetGenericTypeDefinition()));

        private static bool IsDictionaryDefinition(Type definition) =>
            definition == typeof(Dictionary<,>)
            || definition == typeof(IDictionary<,>)
            || definition == typeof(IReadOnlyDictionary<,>);

        // ── equality ─────────────────────────────────────────────────────────────

        /// <summary>Determines whether two <see cref="Shape"/> instances represent the same type contract.</summary>
        /// <param name="other">Shape to compare.</param>
        /// <returns><see langword="true"/> if the shapes are structurally equal.</returns>
        public bool Equals(Shape? other) =>
            other is not null
            && Kind == other.Kind
            && _wrapped.SequenceEqual(other._wrapped)
            && _fields.Count == other._fields.Count
            && _fields.All(field => other._fields.TryGetValue(field.Key, out var shape) && field.Value.Equals(shape));

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as Shape);

        /// <inheritdoc/>
        public override int GetHashCode() =>
            _wrapped.Aggregate(StringComparer.Ordinal.GetHashCode(Kind), (hash, wrapped) => unchecked(hash * 31 + wrapped.GetHashCode()));

        /// <summary>Compares two shapes for structural equality.</summary>
        public static bool operator ==(Shape? left, Shape? right) => Equals(left, right);
        /// <summary>Compares two shapes for structural inequality.</summary>
        public static bool operator !=(Shape? left, Shape? right) => !Equals(left, right);
    }
}
