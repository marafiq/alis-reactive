using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Alis.Reactive.PlanModel
{
    /// <summary>
    /// The type contract the plan expresses. Declares what a value IS and enables
    /// Shape-to-Shape conversion. One shared type used everywhere: JsType properties,
    /// ValueProducer reads, conditions, validation rules, gather.
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
            if (type == typeof(DateTime) || type == typeof(DateTimeOffset)
#if NET6_0_OR_GREATER
                || type == typeof(DateOnly)
#endif
                ) return Date;
            if (type == typeof(byte) || type == typeof(sbyte) ||
                type == typeof(short) || type == typeof(ushort) ||
                type == typeof(int) || type == typeof(uint) ||
                type == typeof(long) || type == typeof(ulong) ||
                type == typeof(float) || type == typeof(double) || type == typeof(decimal))
                return Number;

            if (type == typeof(Guid) || type == typeof(TimeSpan)
#if NET6_0_OR_GREATER
                || type == typeof(TimeOnly)
#endif
                )
                return String;
            if (type.IsEnum)
                return String;

            if (type.IsArray || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)))
            {
                var elementType = type.IsArray ? type.GetElementType()! : type.GetGenericArguments()[0];
                return ArrayOf(FromClrType(elementType));
            }

            return Any;
        }

        public string Kind { get; }
        public Shape Item { get; private set; }
        public Shape Inner { get; private set; }
        public IReadOnlyDictionary<string, Shape> Fields { get; private set; }
        public bool? Additional { get; private set; }

        internal bool IsNone => Kind == "none";

        private Shape(string kind)
        {
            Kind = kind;
        }

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

        public override bool Equals(object obj) => Equals(obj as Shape);

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
                hash = hash * 31 + (Additional.HasValue ? Additional.Value.GetHashCode() : 0);
                return hash;
            }
#endif
        }

        public static bool operator ==(Shape left, Shape right) => Equals(left, right);
        public static bool operator !=(Shape left, Shape right) => !Equals(left, right);
    }
}
