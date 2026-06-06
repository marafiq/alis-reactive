using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.PlanModel
{
    internal sealed class ShapeJsonConverter : JsonConverter<Shape>
    {
        public override void Write(Utf8JsonWriter writer, Shape value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("kind", value.Kind);
            value.WriteContractDetails(writer, options);
            writer.WriteEndObject();
        }

        public override Shape Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            throw new NotSupportedException("Plan types are write-only.");
    }

    /// <summary>
    /// Declares the expected type for a value in the Reactive Plan, such as string or number.
    /// Used across value expressions, conditions, and validation to ensure type consistency.
    /// </summary>
    [JsonConverter(typeof(ShapeJsonConverter))]
    public sealed class Shape : IEquatable<Shape>
    {
        internal static readonly Shape String = Scalar("string");
        internal static readonly Shape Number = Scalar("number");
        internal static readonly Shape Boolean = Scalar("boolean");
        internal static readonly Shape Date = Scalar("date");
        internal static readonly Shape Raw = Scalar("raw");
        internal static readonly Shape Any = Scalar("any");
        internal static readonly Shape None = Scalar("none");

        internal static Shape ArrayOf(Shape item)
        {
            if (item is null)
                throw new ArgumentException("Array item shape is required.", nameof(item));
            if (item.IsNone)
                throw new ArgumentException("Array item shape is required.", nameof(item));
            return new Shape("array", ShapeStructure.Array(item));
        }

        internal static Shape ObjectOf(Dictionary<string, Shape> fields)
        {
            if (fields is null)
                throw new ArgumentNullException(nameof(fields));
            return new Shape("object", ShapeStructure.Object(new ReadOnlyDictionary<string, Shape>(fields)));
        }

        internal static Shape OpenObject()
        {
            return new Shape("object", ShapeStructure.OpenObject);
        }

        internal static Shape Nullable(Shape inner)
        {
            if (inner is null)
                throw new ArgumentException("Nullable inner shape is required.", nameof(inner));
            if (inner.IsNone)
                throw new ArgumentException("Nullable inner shape is required.", nameof(inner));
            return new Shape("nullable", ShapeStructure.Nullable(inner));
        }

        internal static Shape FromClrType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return Nullable(FromClrType(underlying));

            if (type == typeof(string)) return String;
            if (type == typeof(bool)) return Boolean;
            if (IsDateType(type)) return Date;
            if (IsNumericType(type)) return Number;
            if (IsStringSerializedType(type)) return String;
            if (type.IsEnum) return String;

            if (TryGetCollectionItemShape(type, out var itemShape))
                return ArrayOf(itemShape);

            return Any;
        }

        internal static Shape CollectionItemShapeOrNone(Type type) =>
            TryGetCollectionItemShape(type, out var itemShape)
                ? itemShape
                : None;

        internal static Shape FromValue(object? value) =>
            value == null ? None : FromClrType(value.GetType());

        private static bool IsDateType(Type type)
            => type == typeof(DateTime) || type == typeof(DateTimeOffset)
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
            => type == typeof(Guid) || type == typeof(TimeSpan)
#if NET6_0_OR_GREATER
               || type == typeof(TimeOnly)
#endif
               ;

        private static bool TryGetCollectionItemShape(Type type, out Shape itemShape)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (type == typeof(string))
            {
                itemShape = None;
                return false;
            }

            if (type.IsArray)
                return TryGetArrayItemShape(type, out itemShape);

            if (IsDictionaryLike(type))
            {
                itemShape = None;
                return false;
            }

            if (TryGetElementTypeFromSupportedGeneric(type, out var directElementType))
            {
                itemShape = FromClrType(directElementType);
                return true;
            }

            var candidates = type.GetInterfaces()
                .Where(IsSupportedGenericCollection)
                .Select(item => item.GetGenericArguments()[0])
                .Distinct()
                .ToArray();

            if (candidates.Length == 1)
            {
                itemShape = FromClrType(candidates[0]);
                return true;
            }

            itemShape = None;
            return false;
        }

        private static bool TryGetArrayItemShape(Type arrayType, out Shape itemShape)
        {
            var arrayElementType = arrayType.GetElementType();
            if (arrayElementType == null)
            {
                itemShape = None;
                return false;
            }

            itemShape = FromClrType(arrayElementType);
            return true;
        }

        private static bool TryGetElementTypeFromSupportedGeneric(Type type, out Type elementType)
        {
            if (type.IsGenericType && IsSupportedGenericCollection(type))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }

            elementType = typeof(object);
            return false;
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

        private static bool IsDictionaryLike(Type type)
        {
            if (type.IsGenericType && IsDictionaryDefinition(type.GetGenericTypeDefinition()))
                return true;

            return type.GetInterfaces()
                .Any(item => item.IsGenericType && IsDictionaryDefinition(item.GetGenericTypeDefinition()));
        }

        private static bool IsDictionaryDefinition(Type definition) =>
            definition == typeof(Dictionary<,>)
            || definition == typeof(IDictionary<,>)
            || definition == typeof(IReadOnlyDictionary<,>);

        /// <summary>JSON shape kind, such as <c>string</c>, <c>array</c>, <c>object</c>, or <c>nullable</c>.</summary>
        public string Kind { get; }

        internal bool IsNone => Kind == "none";
        internal bool TryGetArrayItemShape(out Shape itemShape) =>
            _structure.TryGetArrayItemShape(out itemShape);

        internal bool TryGetObjectContract(out ShapeObjectContract contract) =>
            _structure.TryGetObjectContract(out contract);

        /// <summary>
        /// True when this shape represents a value that can be meaningfully serialized
        /// to a single string. Suitable for HTTP headers, route params, and query strings.
        /// Scalars: string, number, boolean, date. Nullable wrapping a scalar is also scalar.
        /// Non-scalars: array, object, raw, any, none.
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
                        return true;
                    default:
                        return _structure.IsScalar;
                }
            }
        }

        private readonly ShapeStructure _structure;

        private Shape(string kind, ShapeStructure structure)
        {
            Kind = kind;
            _structure = structure ?? throw new ArgumentNullException(nameof(structure));
        }

        private static Shape Scalar(string kind) => new Shape(kind, ShapeStructure.None);

        internal bool IsNullableOf(Shape inner)
        {
            if (inner is null)
                throw new ArgumentNullException(nameof(inner));

            return _structure.IsNullableWrapping(inner);
        }

        internal bool TryGetNullableInnerShape(out Shape inner) =>
            _structure.TryGetNullableInnerShape(out inner);

        internal string DescribeContract()
        {
            if (TryGetArrayItemShape(out var itemShape))
                return "array<" + itemShape.DescribeContract() + ">";

            if (TryGetNullableInnerShape(out var innerShape))
                return "nullable<" + innerShape.DescribeContract() + ">";

            if (TryGetObjectContract(out var objectContract))
                return objectContract.Describe();

            return Kind;
        }

        internal void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
            _structure.WriteContractDetails(writer, options);

        /// <summary>Determines whether two <see cref="Shape"/> instances represent the same type contract.</summary>
        /// <param name="other">Shape to compare.</param>
        /// <returns><see langword="true"/> if the shapes are structurally equal.</returns>
        public bool Equals(Shape? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Kind != other.Kind) return false;
            return _structure.HasSameContract(other._structure);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as Shape);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
#if NET6_0_OR_GREATER
            return HashCode.Combine(Kind, _structure.GetContractHashCode());
#else
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Kind.GetHashCode();
                hash = hash * 31 + _structure.GetContractHashCode();
                return hash;
            }
#endif
        }

        /// <summary>Compares two shapes for structural equality.</summary>
        public static bool operator ==(Shape? left, Shape? right) => Equals(left, right);
        /// <summary>Compares two shapes for structural inequality.</summary>
        public static bool operator !=(Shape? left, Shape? right) => !Equals(left, right);
    }

    internal abstract class ShapeStructure
    {
        internal static ShapeStructure None { get; } =
            new EmptyShapeStructure();

        internal static ShapeStructure OpenObject { get; } =
            new OpenObjectShapeStructure();

        internal virtual bool IsScalar => false;

        internal virtual bool HasSameContract(ShapeStructure other) =>
            other.GetType() == GetType();

        internal virtual int GetContractHashCode() => GetType().GetHashCode();

        internal virtual bool IsNullableWrapping(Shape inner) => false;

        internal virtual bool TryGetArrayItemShape(out Shape itemShape)
        {
            itemShape = Shape.None;
            return false;
        }

        internal virtual bool TryGetObjectContract(out ShapeObjectContract contract)
        {
            contract = ShapeObjectContract.None;
            return false;
        }

        internal virtual bool TryGetNullableInnerShape(out Shape inner)
        {
            inner = Shape.None;
            return false;
        }

        internal virtual void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
        }

        internal static ShapeStructure Array(Shape item) => new ArrayShapeStructure(item);

        internal static ShapeStructure Nullable(Shape inner) => new NullableShapeStructure(inner);

        internal static ShapeStructure Object(IReadOnlyDictionary<string, Shape> fields) =>
            new ObjectShapeStructure(fields);

        private sealed class EmptyShapeStructure : ShapeStructure
        {
        }

        private sealed class OpenObjectShapeStructure : ShapeStructure
        {
            internal override bool TryGetObjectContract(out ShapeObjectContract contract)
            {
                contract = ShapeObjectContract.Open;
                return true;
            }

            internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                ShapeObjectContract.Open.WriteTo(writer, options);
        }

        private sealed class ArrayShapeStructure : ShapeStructure
        {
            private readonly Shape _item;

            internal ArrayShapeStructure(Shape item)
            {
                _item = item ?? throw new ArgumentNullException(nameof(item));
            }

            internal override bool HasSameContract(ShapeStructure other) =>
                other is ArrayShapeStructure array && _item.Equals(array._item);

            internal override int GetContractHashCode() => _item.GetHashCode();

            internal override bool TryGetArrayItemShape(out Shape itemShape)
            {
                itemShape = _item;
                return true;
            }

            internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
                writer.WritePropertyName("item");
                JsonSerializer.Serialize(writer, _item, options);
            }
        }

        private sealed class NullableShapeStructure : ShapeStructure
        {
            private readonly Shape _inner;

            internal NullableShapeStructure(Shape inner)
            {
                _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            }

            internal override bool IsScalar => _inner.IsScalar;

            internal override bool HasSameContract(ShapeStructure other) =>
                other is NullableShapeStructure nullable && _inner.Equals(nullable._inner);

            internal override int GetContractHashCode() => _inner.GetHashCode();

            internal override bool IsNullableWrapping(Shape inner) => _inner.Equals(inner);

            internal override bool TryGetNullableInnerShape(out Shape inner)
            {
                inner = _inner;
                return true;
            }

            internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options)
            {
                writer.WritePropertyName("inner");
                JsonSerializer.Serialize(writer, _inner, options);
            }
        }

        private sealed class ObjectShapeStructure : ShapeStructure
        {
            private readonly IReadOnlyDictionary<string, Shape> _fields;

            internal ObjectShapeStructure(IReadOnlyDictionary<string, Shape> fields)
            {
                _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            }

            internal override bool HasSameContract(ShapeStructure other)
            {
                var otherObject = other as ObjectShapeStructure;
                if (otherObject == null) return false;
                if (_fields.Count != otherObject._fields.Count) return false;

                foreach (var field in _fields)
                {
                    var otherHasField = otherObject._fields.TryGetValue(field.Key, out var otherShape);
                    if (!otherHasField) return false;
                    if (!field.Value.Equals(otherShape)) return false;
                }

                return true;
            }

            internal override int GetContractHashCode()
            {
                unchecked
                {
                    var hash = GetType().GetHashCode();
                    foreach (var field in _fields)
                    {
                        hash = (hash * 397) ^ field.Key.GetHashCode();
                        hash = (hash * 397) ^ field.Value.GetHashCode();
                    }

                    return hash;
                }
            }

            internal override bool TryGetObjectContract(out ShapeObjectContract contract)
            {
                contract = ShapeObjectContract.Closed(_fields);
                return true;
            }

            internal override void WriteContractDetails(Utf8JsonWriter writer, JsonSerializerOptions options) =>
                ShapeObjectContract.Closed(_fields).WriteTo(writer, options);
        }
    }

    internal sealed class ShapeObjectContract
    {
        private static readonly IReadOnlyDictionary<string, Shape> NoDeclaredFields =
            new ReadOnlyDictionary<string, Shape>(new Dictionary<string, Shape>());

        internal static readonly ShapeObjectContract Open =
            new ShapeObjectContract(NoDeclaredFields, additional: true);

        internal static readonly ShapeObjectContract None =
            new ShapeObjectContract(NoDeclaredFields, additional: false);

        private readonly IReadOnlyDictionary<string, Shape> _fields;
        private readonly bool _additional;

        private ShapeObjectContract(IReadOnlyDictionary<string, Shape> fields, bool additional)
        {
            _fields = fields ?? throw new ArgumentNullException(nameof(fields));
            _additional = additional;
        }

        internal IReadOnlyDictionary<string, Shape> Fields => _fields;

        internal bool AllowsAdditionalFields => _additional;

        internal static ShapeObjectContract Closed(IReadOnlyDictionary<string, Shape> fields) =>
            new ShapeObjectContract(fields, additional: false);

        internal string Describe()
        {
            if (_fields.Count == 0)
                return _additional ? "object<open>" : "object{}";

            var description = new StringBuilder();
            description.Append("object{");
            var first = true;
            foreach (var field in _fields)
            {
                if (!first) description.Append(", ");
                description.Append(field.Key);
                description.Append(":");
                description.Append(field.Value.DescribeContract());
                first = false;
            }

            if (_additional)
            {
                if (!first) description.Append(", ");
                description.Append("...");
            }

            description.Append("}");
            return description.ToString();
        }

        internal void WriteTo(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            writer.WritePropertyName("fields");
            writer.WriteStartObject();
            foreach (var field in _fields)
            {
                writer.WritePropertyName(field.Key);
                JsonSerializer.Serialize(writer, field.Value, options);
            }
            writer.WriteEndObject();

            writer.WriteBoolean("additional", _additional);
        }
    }
}
