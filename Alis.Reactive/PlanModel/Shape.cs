using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.PlanModel
{
    /// <summary>Single source of truth for the legal shape kind discriminator strings.</summary>
    internal static class ShapeKinds
    {
        internal const string String   = "string";
        internal const string Number   = "number";
        internal const string Boolean  = "boolean";
        internal const string Date     = "date";
        internal const string Raw      = "raw";
        internal const string Any      = "any";
        internal const string None     = "none";
        internal const string Array    = "array";
        internal const string Nullable = "nullable";
        internal const string Object   = "object";
    }

    /// <summary>
    /// Declares the expected type for a value in the plan (string, number, date, array, object, etc.).
    /// Construct shapes through framework builders — <c>Shape</c> instances are produced by
    /// <c>Html.InputField</c>, gather/condition builders, and validator extraction. Consumers do
    /// not pattern-match on shapes; the framework owns shape semantics end-to-end.
    /// </summary>
    [JsonConverter(typeof(WriteOnlyPolymorphicConverter<Shape>))]
    public abstract class Shape : IEquatable<Shape>
    {
        // ── singleton instances (identity preserved from previous design) ──
        internal static readonly Shape String  = new ScalarShape(ShapeKinds.String);
        internal static readonly Shape Number  = new ScalarShape(ShapeKinds.Number);
        internal static readonly Shape Boolean = new ScalarShape(ShapeKinds.Boolean);
        internal static readonly Shape Date    = new ScalarShape(ShapeKinds.Date);
        internal static readonly Shape Raw     = new OpaqueShape(ShapeKinds.Raw);
        internal static readonly Shape Any     = new OpaqueShape(ShapeKinds.Any);
        internal static readonly Shape None    = new NoneShape();

        // ── factories (signatures unchanged from previous design) ──
        internal static Shape ArrayOf(Shape item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item is NoneShape)
                throw new ArgumentException("Array item shape is required.", nameof(item));
            return new ArrayShape(item);
        }

        internal static Shape ObjectOf(Dictionary<string, Shape> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            return new ObjectShape(new ReadOnlyDictionary<string, Shape>(fields));
        }

        internal static Shape Nullable(Shape inner)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (inner is NoneShape)
                throw new ArgumentException("Nullable inner shape is required.", nameof(inner));
            return new NullableShape(inner);
        }

        internal static Shape FromClrType(Type type)
        {
            if (type == null) return Any;

            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null) return Nullable(FromClrType(underlying));

            if (type == typeof(string)) return String;
            if (type == typeof(bool)) return Boolean;
            if (IsDateType(type)) return Date;
            if (IsNumericType(type)) return Number;
            if (IsStringSerializedType(type)) return String;
            if (type.IsEnum) return String;

            var elementType = GetCollectionElementType(type);
            if (elementType != null) return ArrayOf(FromClrType(elementType));

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

        private static Type? GetCollectionElementType(Type type)
        {
            if (type.IsArray) return type.GetElementType()!;
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                return type.GetGenericArguments()[0];
            return null;
        }

        /// <summary>Gets the shape kind (string, number, boolean, date, array, object, nullable, raw, any, or none).</summary>
        public abstract string Kind { get; }

        internal abstract bool IsScalar { get; }

        internal bool IsNone => this is NoneShape;

        // Explicit private protected ctor matches RequestInput precedent. Prevents external
        // assemblies from attempting `class MyShape : Shape` derivation.
        private protected Shape() { }

        /// <summary>Determines whether two <see cref="Shape"/> instances represent the same type contract.</summary>
        /// <param name="other">The shape to compare with.</param>
        /// <returns><see langword="true"/> if the shapes are structurally equal.</returns>
        public bool Equals(Shape? other)
            => !(other is null) && other.GetType() == GetType() && EqualsSameType(other);

        /// <summary>
        /// Same-type structural equality. The base <see cref="Equals(Shape?)"/> guarantees
        /// <paramref name="other"/> is a non-null instance of the same runtime type before invoking this method.
        /// </summary>
        /// <remarks>
        /// Implementations MUST cast <paramref name="other"/> to their own type unconditionally
        /// and MUST NOT add their own type checks. Adding an <c>is X</c> check here is dead
        /// code that will mask logic errors. The base contract is: same <c>GetType()</c> OR
        /// this method is not called.
        /// </remarks>
        protected abstract bool EqualsSameType(Shape other);

        /// <inheritdoc/>
        public override bool Equals(object? obj) => obj is Shape s && Equals(s);

        /// <inheritdoc/>
        public abstract override int GetHashCode();

        /// <summary>Returns <see langword="true"/> if both shapes are structurally equal.</summary>
        public static bool operator ==(Shape? left, Shape? right)
            => ReferenceEquals(left, right) || (!(left is null) && left.Equals(right));

        /// <summary>Returns <see langword="true"/> if the shapes are not structurally equal.</summary>
        public static bool operator !=(Shape? left, Shape? right) => !(left == right);
    }

    /// <summary>A leaf shape representing a primitive value (string, number, boolean, or date).</summary>
    internal sealed class ScalarShape : Shape
    {
        public override string Kind { get; }
        internal override bool IsScalar => true;

        internal ScalarShape(string kind)
        {
            if (kind != ShapeKinds.String && kind != ShapeKinds.Number
                && kind != ShapeKinds.Boolean && kind != ShapeKinds.Date)
                throw new ArgumentException(
                    "Invalid scalar kind '" + kind + "'. Must be one of: string, number, boolean, date.",
                    nameof(kind));
            Kind = kind;
        }

        protected override bool EqualsSameType(Shape other) => Kind == ((ScalarShape)other).Kind;

#if NET6_0_OR_GREATER
        public override int GetHashCode() => Kind.GetHashCode();
#else
        public override int GetHashCode() { unchecked { return Kind.GetHashCode(); } }
#endif
    }

    /// <summary>A leaf shape representing a non-scalar untyped value: raw JSON or untyped any.</summary>
    internal sealed class OpaqueShape : Shape
    {
        public override string Kind { get; }
        internal override bool IsScalar => false;

        internal OpaqueShape(string kind)
        {
            if (kind != ShapeKinds.Raw && kind != ShapeKinds.Any)
                throw new ArgumentException(
                    "Invalid opaque kind '" + kind + "'. Must be one of: raw, any.",
                    nameof(kind));
            Kind = kind;
        }

        protected override bool EqualsSameType(Shape other) => Kind == ((OpaqueShape)other).Kind;

#if NET6_0_OR_GREATER
        public override int GetHashCode() => Kind.GetHashCode();
#else
        public override int GetHashCode() { unchecked { return Kind.GetHashCode(); } }
#endif
    }

    /// <summary>The absence of a shape. Singleton; type-enforced uniqueness via factory guards.</summary>
    internal sealed class NoneShape : Shape
    {
        public override string Kind => ShapeKinds.None;
        internal override bool IsScalar => false;

        internal NoneShape() { }

        // Singleton semantics: any two NoneShape instances are structurally equal.
        protected override bool EqualsSameType(Shape other) => true;

#if NET6_0_OR_GREATER
        public override int GetHashCode() => ShapeKinds.None.GetHashCode();
#else
        public override int GetHashCode() { unchecked { return ShapeKinds.None.GetHashCode(); } }
#endif
    }

    /// <summary>A shape describing an array whose elements all match <see cref="Item"/>.</summary>
    internal sealed class ArrayShape : Shape
    {
        public override string Kind => ShapeKinds.Array;
        internal override bool IsScalar => false;

        /// <summary>Gets the element shape that every array item must match.</summary>
        public Shape Item { get; }

        internal ArrayShape(Shape item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            Item = item;
        }

        protected override bool EqualsSameType(Shape other) => Item.Equals(((ArrayShape)other).Item);

#if NET6_0_OR_GREATER
        public override int GetHashCode() => HashCode.Combine(ShapeKinds.Array, Item);
#else
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ShapeKinds.Array.GetHashCode();
                hash = hash * 31 + Item.GetHashCode();
                return hash;
            }
        }
#endif
    }

    /// <summary>A shape that wraps another shape to express that the value may be null.</summary>
    internal sealed class NullableShape : Shape
    {
        public override string Kind => ShapeKinds.Nullable;
        internal override bool IsScalar => Inner.IsScalar;

        /// <summary>Gets the wrapped shape that the value matches when not null.</summary>
        public Shape Inner { get; }

        internal NullableShape(Shape inner)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            Inner = inner;
        }

        protected override bool EqualsSameType(Shape other) => Inner.Equals(((NullableShape)other).Inner);

#if NET6_0_OR_GREATER
        public override int GetHashCode() => HashCode.Combine(ShapeKinds.Nullable, Inner);
#else
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ShapeKinds.Nullable.GetHashCode();
                hash = hash * 31 + Inner.GetHashCode();
                return hash;
            }
        }
#endif
    }

    /// <summary>A shape describing an object with named field shapes.</summary>
    internal sealed class ObjectShape : Shape
    {
        public override string Kind => ShapeKinds.Object;
        internal override bool IsScalar => false;

        /// <summary>Gets the named field shapes for this object shape.</summary>
        public IReadOnlyDictionary<string, Shape> Fields { get; }

        internal ObjectShape(IReadOnlyDictionary<string, Shape> fields)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            foreach (var kvp in fields)
            {
                if (kvp.Value == null)
                    throw new ArgumentException(
                        "Field shape for key '" + kvp.Key + "' is null. Object shape field values must be non-null.",
                        nameof(fields));
            }
            Fields = fields;
        }

        protected override bool EqualsSameType(Shape other)
        {
            var o = (ObjectShape)other;
            if (Fields.Count != o.Fields.Count) return false;
            foreach (var kvp in Fields)
            {
                if (!o.Fields.TryGetValue(kvp.Key, out var v) || !kvp.Value.Equals(v))
                    return false;
            }
            return true;
        }

        // Combines "object", Count, and an order-independent XOR over key hashes.
        // O(N), allocation-free, order-independent. Field-shape hash codes intentionally NOT
        // included — would force every nested shape to evaluate its hash on every parent hash.
        // Keys alone distinguish almost all real shapes. The Equals contract is still respected
        // (Equals-equal implies Hash-equal): two ObjectShapes with identical key sets and
        // identical field shapes hash identically and Equals-equal.
        public override int GetHashCode()
        {
            int keyHash = 0;
            foreach (var key in Fields.Keys)
                keyHash ^= StringComparer.Ordinal.GetHashCode(key);
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + ShapeKinds.Object.GetHashCode();
                hash = hash * 31 + Fields.Count;
                hash = hash * 31 + keyHash;
                return hash;
            }
        }
    }
}
