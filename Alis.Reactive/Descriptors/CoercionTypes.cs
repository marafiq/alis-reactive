using System;
using System.Collections.Generic;

namespace Alis.Reactive.Descriptors
{
    /// <summary>
    /// Constants and inference logic for the <c>coerce</c> field in the JSON plan.
    /// The runtime uses coercion to convert string values to the correct JavaScript type.
    /// </summary>
    public static class CoercionTypes
    {
        /// <summary>Coerce to JavaScript string.</summary>
        public const string String = "string";
        /// <summary>Coerce to JavaScript number.</summary>
        public const string Number = "number";
        /// <summary>Coerce to JavaScript boolean.</summary>
        public const string Boolean = "boolean";
        /// <summary>Coerce to JavaScript Date.</summary>
        public const string Date = "date";
        /// <summary>Pass the value through without coercion.</summary>
        public const string Raw = "raw";
        /// <summary>Coerce to JavaScript array.</summary>
        public const string Array = "array";

        /// <summary>
        /// Infers the coercion type from a .NET <see cref="Type"/>.
        /// Maps CLR primitives to their JavaScript equivalents.
        /// </summary>
        /// <param name="type">The .NET type to infer coercion for.</param>
        /// <returns>One of the coercion type constants.</returns>
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
            if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset) ||
                underlying == typeof(DateOnly)) return Date;
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
