using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Alis.Reactive.Serialization;

namespace Alis.Reactive.Grammar
{
    /// <summary>
    /// The type of a value in the plan, as MVC binds it (grammar.md 2.1): a scalar (string, number,
    /// boolean, date, instant), an array of a shape, a nullable shape, an object of named shapes, any
    /// value, or none. Ten productions, one node each, written and declared like every other node.
    /// </summary>
    [JsonConverter(typeof(PlanNodeDiscriminator<Shape>))]
    internal abstract class Shape : IEquatable<Shape>
    {
        internal static readonly Shape String = new StringShape();
        internal static readonly Shape Number = new NumberShape();
        internal static readonly Shape Boolean = new BooleanShape();
        internal static readonly Shape Date = new DateShape();
        /// <summary>A moment in time with its offset (<see cref="DateTimeOffset"/>), sent as the exact instant.</summary>
        internal static readonly Shape Instant = new InstantShape();
        internal static readonly Shape Any = new AnyShape();
        internal static readonly Shape None = new NoneShape();

        private protected Shape() { }

        /// <summary>JSON shape kind, such as <c>string</c>, <c>array</c>, <c>object</c>, or <c>nullable</c>.</summary>
        public abstract string Kind { get; }

        internal bool IsNone => this is NoneShape;

        /// <summary>Whether a value of this shape is a list, through a nullable wrapper too.</summary>
        internal bool DescribesArray => this is NullableShape nullable ? nullable.Inner.DescribesArray : this is ArrayShape;

        /// <summary>
        /// True when a value of this shape serializes to one string, as an HTTP header, a route parameter or a
        /// query value needs: string, number, boolean, date, instant, or a nullable of one of them.
        /// </summary>
        internal bool IsScalar => this switch
        {
            NullableShape nullable => nullable.Inner.IsScalar,
            StringShape or NumberShape or BooleanShape or DateShape or InstantShape => true,
            _ => false,
        };

        internal static Shape ArrayOf(Shape item) => new ArrayShape(Wrappable(item, nameof(item)));

        internal static Shape Nullable(Shape inner) => new NullableShape(Wrappable(inner, nameof(inner)));

        internal static Shape ObjectOf(Dictionary<string, Shape> fields)
        {
            if (fields is null) throw new ArgumentNullException(nameof(fields));
            return new ObjectShape(new ReadOnlyDictionary<string, Shape>(fields));
        }

        private static Shape Wrappable(Shape shape, string parameter)
        {
            if (shape is null || shape.IsNone)
                throw new ArgumentException($"The {parameter} shape is required.", parameter);
            return shape;
        }

        internal bool TryGetArrayItemShape(out Shape item)
        {
            item = this is ArrayShape array ? array.Item : None;
            return this is ArrayShape;
        }

        internal bool TryGetNullableInnerShape(out Shape inner)
        {
            inner = this is NullableShape nullable ? nullable.Inner : None;
            return this is NullableShape;
        }

        internal string DescribeContract() => this switch
        {
            ArrayShape array => "array<" + array.Item.DescribeContract() + ">",
            NullableShape nullable => "nullable<" + nullable.Inner.DescribeContract() + ">",
            ObjectShape record => "object{" + string.Join(", ", record.Fields.Select(field => field.Key + ":" + field.Value.DescribeContract())) + "}",
            _ => Kind,
        };

        // ── from CLR types ───────────────────────────────────────────────────────

        internal static Shape FromClrType(Type type) => Of(type, enclosingRecords: null);

        /// <summary>
        /// The shape a method declares for an argument of this type: as <see cref="FromClrType"/>, except
        /// that a class of the app's own data (a server's class, a row, an event) is an object of its
        /// members by their JSON names, so a date member reaches the method as a <c>Date</c> (grammar.md
        /// 1.4, 2.1: a call argument converts to the shape the member declares).
        /// </summary>
        internal static Shape OfArgument(Type type) => Of(type, enclosingRecords: new HashSet<Type>());

        // With enclosingRecords, a data class is a record of its members; the set holds the classes being
        // described, so a class that contains itself is any where it recurs.
        private static Shape Of(Type type, HashSet<Type>? enclosingRecords)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return Nullable(Of(underlying, enclosingRecords));

            if (type == typeof(string)) return String;
            if (type == typeof(bool)) return Boolean;
            if (type == typeof(DateTimeOffset)) return Instant;
            if (IsDateType(type)) return Date;
            if (IsNumericType(type)) return Number;
            if (IsStringSerializedType(type)) return String;
            if (type.IsEnum) return String;

            if (TryGetCollectionItemShape(type, enclosingRecords, out var itemShape))
                return ArrayOf(itemShape);

            var isRecord = enclosingRecords != null && IsDataClass(type) && enclosingRecords.Add(type);
            return isRecord ? RecordOf(type, enclosingRecords!) : Any;
        }

        private static Shape RecordOf(Type type, HashSet<Type> enclosingRecords)
        {
            var fields = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var serialized = property.GetIndexParameters().Length == 0
                    && property.GetMethod != null
                    && property.GetCustomAttribute<JsonIgnoreAttribute>() == null;
                if (serialized) fields[JsonMemberName.Of(property.Name)] = Of(property.PropertyType, enclosingRecords);
            }
            enclosingRecords.Remove(type);
            return ObjectOf(fields);
        }

        // Data the app defines: not a live JavaScript object (a declaration), not a framework or library
        // type, not a dictionary (JSON of any keys).
        private static bool IsDataClass(Type type)
        {
            var ns = type.Namespace ?? "";
            return type.IsClass
                   && type != typeof(object)
                   && !type.IsAbstract
                   && !typeof(JsObject).IsAssignableFrom(type)
                   && !typeof(Delegate).IsAssignableFrom(type)
                   && !IsDictionaryLike(type)
                   && !ns.StartsWith("System", StringComparison.Ordinal)
                   && !ns.StartsWith("Microsoft", StringComparison.Ordinal);
        }

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
        private static bool TryGetCollectionItemShape(Type type, HashSet<Type>? enclosingRecords, out Shape itemShape)
        {
            itemShape = None;
            var bindsAsList = type != typeof(string) && !IsDictionaryLike(type);
            if (!bindsAsList || !TryGetItemType(type, out var itemType)) return false;

            itemShape = Of(itemType, enclosingRecords);
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
        public bool Equals(Shape? other) => other switch
        {
            null => false,
            ArrayShape array => this is ArrayShape mine && mine.Item.Equals(array.Item),
            NullableShape nullable => this is NullableShape mine && mine.Inner.Equals(nullable.Inner),
            ObjectShape record => this is ObjectShape mine && SameFields(mine.Fields, record.Fields),
            _ => Kind == other.Kind,
        };

        private static bool SameFields(IReadOnlyDictionary<string, Shape> mine, IReadOnlyDictionary<string, Shape> theirs) =>
            mine.Count == theirs.Count
            && mine.All(field => theirs.TryGetValue(field.Key, out var shape) && field.Value.Equals(shape));

        /// <inheritdoc/>
        public override bool Equals(object? obj) => Equals(obj as Shape);

        /// <inheritdoc/>
        public override int GetHashCode() => this switch
        {
            ArrayShape array => unchecked(StringComparer.Ordinal.GetHashCode(Kind) * 31 + array.Item.GetHashCode()),
            NullableShape nullable => unchecked(StringComparer.Ordinal.GetHashCode(Kind) * 31 + nullable.Inner.GetHashCode()),
            _ => StringComparer.Ordinal.GetHashCode(Kind),
        };

        /// <summary>Compares two shapes for structural equality.</summary>
        public static bool operator ==(Shape? left, Shape? right) => Equals(left, right);
        /// <summary>Compares two shapes for structural inequality.</summary>
        public static bool operator !=(Shape? left, Shape? right) => !Equals(left, right);
    }

    internal sealed class StringShape : Shape
    {
        public override string Kind => "string";
    }

    internal sealed class NumberShape : Shape
    {
        public override string Kind => "number";
    }

    internal sealed class BooleanShape : Shape
    {
        public override string Kind => "boolean";
    }

    internal sealed class DateShape : Shape
    {
        public override string Kind => "date";
    }

    internal sealed class InstantShape : Shape
    {
        public override string Kind => "instant";
    }

    /// <summary>A value of no declared shape, converted by nothing.</summary>
    internal sealed class AnyShape : Shape
    {
        public override string Kind => "any";
    }

    /// <summary>No value: what a method without a result or a payload without a body reads.</summary>
    internal sealed class NoneShape : Shape
    {
        public override string Kind => "none";
    }

    /// <summary>A list whose items are one shape.</summary>
    internal sealed class ArrayShape : Shape
    {
        internal ArrayShape(Shape item)
        {
            Item = item;
        }

        public override string Kind => "array";
        public Shape Item { get; }
    }

    /// <summary>A shape or nothing, as MVC binds a nullable.</summary>
    internal sealed class NullableShape : Shape
    {
        internal NullableShape(Shape inner)
        {
            Inner = inner;
        }

        public override string Kind => "nullable";
        public Shape Inner { get; }
    }

    /// <summary>An object of named shapes, by their JSON names.</summary>
    internal sealed class ObjectShape : Shape
    {
        internal ObjectShape(IReadOnlyDictionary<string, Shape> fields)
        {
            Fields = fields;
        }

        public override string Kind => "object";
        public IReadOnlyDictionary<string, Shape> Fields { get; }
    }
}
