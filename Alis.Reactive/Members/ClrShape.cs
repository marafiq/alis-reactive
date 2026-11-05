using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Alis.Reactive.Grammar;

namespace Alis.Reactive.Members
{
    internal static class ClrShape
    {
        internal static Shape Of(Type type) => Of(type, _ => Shape.Any);

        internal static Shape OfValue(object? value) => value == null ? Shape.Any : Of(value.GetType());

        internal static Shape Of(Type type, Func<Type, Shape> ofClass)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (ofClass == null) throw new ArgumentNullException(nameof(ofClass));

            var underlying = System.Nullable.GetUnderlyingType(type);
            if (underlying != null)
                return Shape.Nullable(Of(underlying, ofClass));

            if (type == typeof(string)) return Shape.String;
            if (type == typeof(bool)) return Shape.Boolean;
            if (type == typeof(DateTimeOffset)) return Shape.Instant;
            if (IsDateType(type)) return Shape.Date;
            if (IsNumericType(type)) return Shape.Number;
            if (IsStringSerializedType(type)) return Shape.String;
            if (type.IsEnum) return Shape.String;

            if (TryGetItemType(type, out var itemType))
                return Shape.ArrayOf(Of(itemType, ofClass));

            return ofClass(type);
        }

        internal static IEnumerable<PropertyInfo> SerializedProperties(Type type)
            => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetIndexParameters().Length == 0
                                   && property.GetMethod != null
                                   && property.GetCustomAttribute<JsonIgnoreAttribute>() == null);

        internal static bool IsDictionaryLike(Type type)
            => new[] { type }.Concat(type.GetInterfaces())
                .Any(candidate => candidate.IsGenericType && IsDictionaryDefinition(candidate.GetGenericTypeDefinition()));

        internal static bool IsDataClass(Type type)
        {
            var ns = type.Namespace ?? "";
            return type.IsClass
                   && type != typeof(object)
                   && !type.IsAbstract
                   && !typeof(Delegate).IsAssignableFrom(type)
                   && !IsDictionaryLike(type)
                   && !ns.StartsWith("System", StringComparison.Ordinal)
                   && !ns.StartsWith("Microsoft", StringComparison.Ordinal);
        }

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

        private static bool TryGetItemType(Type type, out Type itemType)
        {
            itemType = typeof(object);
            if (type == typeof(string) || IsDictionaryLike(type)) return false;

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
            if (itemTypes.Length != 1) return false;
            itemType = itemTypes[0];
            return true;
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

        private static bool IsDictionaryDefinition(Type definition)
            => definition == typeof(Dictionary<,>)
            || definition == typeof(IDictionary<,>)
            || definition == typeof(IReadOnlyDictionary<,>);
    }
}
