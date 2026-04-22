using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// Declares the expected type for a value in the plan (string, number, date, array, object, etc.).
    /// Used across value expressions, conditions, and validation to ensure type consistency.
    /// </summary>
    public sealed class Shape : IEquatable<Shape>
    {
        internal static readonly Shape String = new Shape("string");
        internal static readonly Shape Number = new Shape("number");
        internal static readonly Shape Boolean = new Shape("boolean");
        internal static readonly Shape Date = new Shape("date");
        internal static readonly Shape Raw = new Shape("raw");
        internal static readonly Shape Any = new Shape("any");
        internal static readonly Shape None = new Shape("none");

        internal static Shape ArrayOf(Shape item)
        {
            if (item == null || item == None)
                throw new ArgumentException("Array item shape is required.", nameof(item));
            return new Shape("array") { Item = item };
        }

        internal static Shape ObjectOf(Dictionary<string, Shape> fields)
        {
            return new Shape("object") { Fields = new ReadOnlyDictionary<string, Shape>(fields) };
        }

        internal static Shape OpenObject()
        {
            return new Shape("object") { Additional = true };
        }

        internal static Shape Nullable(Shape inner)
        {
            if (inner == null || inner == None)
                throw new ArgumentException("Nullable inner shape is required.", nameof(inner));
            return new Shape("nullable") { Inner = inner };
        }

        internal static Shape FromClrType(Type type)
        {
            if (type == null) return Any;

            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return Nullable(FromClrType(underlying));

            if (type == typeof(string)) return String;
            if (type == typeof(bool)) return Boolean;
            if (IsDateType(type)) return Date;
            if (IsNumericType(type)) return Number;
            if (IsStringSerializedType(type)) return String;
            if (type.IsEnum) return String;

            var elementType = GetCollectionElementType(type);
            if (elementType != null)
                return ArrayOf(FromClrType(elementType));

            return Any;
        }

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

        private static Type GetCollectionElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType()!;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];
            return null;
        }

        /// <summary>Gets the shape kind (string, number, boolean, date, array, object, nullable, raw, any, or none).</summary>
        public string Kind { get; }
        /// <summary>Gets the element shape for array shapes, or <see langword="null"/> for non-array shapes.</summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public Shape Item { get; private set; }
        /// <summary>Gets the wrapped shape for nullable shapes, or <see langword="null"/> for non-nullable shapes.</summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public Shape Inner { get; private set; }
        /// <summary>Gets the named field shapes for object shapes, or <see langword="null"/> for non-object shapes.</summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyDictionary<string, Shape> Fields { get; private set; }
        /// <summary>Gets whether the object shape accepts properties beyond those listed in <see cref="Fields"/>. When <see langword="true"/>, the value may contain extra keys not declared in the shape. Default <see langword="false"/> means the shape is closed.</summary>
        [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
        public bool Additional { get; private set; }

        internal bool IsNone => Kind == "none";

        /// <summary>
        /// Returns true if this shape represents a value that can be meaningfully serialized
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
                    case "nullable":
                        return Inner?.IsScalar == true;
                    default:
                        return false;
                }
            }
        }

        private Shape(string kind)
        {
            Kind = kind;
        }

        /// <summary>Determines whether two <see cref="Shape"/> instances represent the same type contract.</summary>
        /// <param name="other">The shape to compare with.</param>
        /// <returns><see langword="true"/> if the shapes are structurally equal.</returns>
        public bool Equals(Shape other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Kind != other.Kind) return false;
            if (!Equals(Item, other.Item)) return false;
            if (!Equals(Inner, other.Inner)) return false;
            if (Additional != other.Additional) return false;
            if (Fields == null && other.Fields == null) return true;
            if (Fields == null || other.Fields == null) return false;
            if (Fields.Count != other.Fields.Count) return false;
            foreach (var kvp in Fields)
            {
                if (!other.Fields.TryGetValue(kvp.Key, out var otherShape) || !kvp.Value.Equals(otherShape))
                    return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => Equals(obj as Shape);

        /// <inheritdoc/>
        public override int GetHashCode()
        {
#if NET6_0_OR_GREATER
            return HashCode.Combine(Kind, Item, Inner, Fields?.Count ?? 0, Additional);
#else
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (Kind != null ? Kind.GetHashCode() : 0);
                hash = hash * 31 + (Item != null ? Item.GetHashCode() : 0);
                hash = hash * 31 + (Inner != null ? Inner.GetHashCode() : 0);
                hash = hash * 31 + (Fields?.Count ?? 0);
                hash = hash * 31 + Additional.GetHashCode();
                return hash;
            }
#endif
        }

        /// <summary>Returns <see langword="true"/> if both shapes are structurally equal.</summary>
        public static bool operator ==(Shape left, Shape right) => Equals(left, right);
        /// <summary>Returns <see langword="true"/> if the shapes are not structurally equal.</summary>
        public static bool operator !=(Shape left, Shape right) => !Equals(left, right);

        /// <summary>
        /// Returns true if a value of shape <paramref name="other"/> can be safely assigned
        /// to a target of this shape. Identity and nullable-widening: a T value satisfies
        /// a Nullable(T) target (non-null is a subset of nullable). Array shapes recurse
        /// on their element types. All other cases require strict equality.
        /// </summary>
        /// <remarks>
        /// Used by typed accessor cross-checks so that callers can pass a non-null concrete
        /// value into a nullable-registered field without writing explicit
        /// <c>.SetValue&lt;DateTime?&gt;(new DateTime(...))</c> ceremony.
        /// </remarks>
        internal bool Accepts(Shape other)
        {
            if (other is null) return false;
            if (Equals(other)) return true;
            // Nullable(T) accepts T — passing a non-null value into a nullable-typed slot is always safe.
            if (Kind == "nullable" && Inner != null && Inner.Accepts(other)) return true;
            // Symmetric widening: a Nullable(T) value where the runtime can't produce null
            // can be written into a T-typed slot in a .Value<TProp>() read context.
            if (other.Kind == "nullable" && other.Inner != null && Accepts(other.Inner)) return true;
            // Array widening: Shape.ArrayOf(T) accepts ArrayOf(T') when T.Accepts(T').
            if (Kind == "array" && other.Kind == "array" && Item != null && other.Item != null
                && Item.Accepts(other.Item)) return true;
            return false;
        }

        /// <summary>Human-readable description of the shape — used by typed-accessor error messages.</summary>
        public override string ToString()
        {
            switch (Kind)
            {
                case "array":
                    return Item != null ? $"Array({Item})" : "Array";
                case "nullable":
                    return Inner != null ? $"Nullable({Inner})" : "Nullable";
                case "object":
                    if (Fields != null && Fields.Count > 0)
                    {
                        var inner = string.Join(", ", System.Linq.Enumerable.Select(Fields, kv => kv.Key + ":" + kv.Value));
                        return Additional ? $"Object({{{inner}, ...}})" : $"Object({{{inner}}})";
                    }
                    return Additional ? "Object({...})" : "Object";
                default:
                    return char.ToUpperInvariant(Kind[0]) + Kind.Substring(1);
            }
        }
    }
}
