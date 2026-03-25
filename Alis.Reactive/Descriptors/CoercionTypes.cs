using System;
using System.Collections.Generic;

namespace Alis.Reactive.Descriptors
{
    public static class CoercionTypes
    {
        public const string String = "string";
        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string Date = "date";
        public const string Raw = "raw";
        public const string Array = "array";

        public static string InferFromType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            // Array types BEFORE scalar checks — string[] is an array, not a string.
            if (underlying.IsArray) return Array;
            if (underlying.IsGenericType)
            {
                var genDef = underlying.GetGenericTypeDefinition();
                if (genDef == typeof(List<>) || genDef == typeof(IEnumerable<>) ||
                    genDef == typeof(ICollection<>) || genDef == typeof(IReadOnlyList<>))
                    return Array;
            }

            if (underlying == typeof(string)) return String;
            if (underlying == typeof(bool)) return Boolean;
            if (underlying == typeof(int) || underlying == typeof(long) ||
                underlying == typeof(double) || underlying == typeof(float) ||
                underlying == typeof(decimal) || underlying == typeof(short) ||
                underlying == typeof(byte)) return Number;
            if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset)
#if NET6_0_OR_GREATER
                || underlying == typeof(DateOnly)
#endif
                ) return Date;
            if (underlying.IsEnum) return String;
            return Raw;
        }

        /// <summary>
        /// Infers the coercion type for individual elements of an array/collection type.
        /// For arrays: uses GetElementType() (NOT GetGenericArguments — arrays are not generic).
        /// For generics (List&lt;T&gt;, etc.): uses GetGenericArguments()[0].
        /// For non-collection types: returns Raw.
        /// </summary>
        public static string InferElementType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            if (underlying.IsArray)
                return InferFromType(underlying.GetElementType()!);
            if (underlying.IsGenericType)
            {
                var args = underlying.GetGenericArguments();
                if (args.Length == 1) return InferFromType(args[0]);
            }
            return Raw;
        }
    }
}
