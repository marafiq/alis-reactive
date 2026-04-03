using System;
using System.Collections.Generic;

namespace Alis.Reactive
{
    /// <summary>
    /// Canonical type hints used when the authored plan needs an explicit runtime conversion.
    /// </summary>
    public static class CoercionTypes
    {
        /// <summary>Canonical hint for string values.</summary>
        public const string String = "string";
        /// <summary>Canonical hint for numeric values.</summary>
        public const string Number = "number";
        /// <summary>Canonical hint for boolean values.</summary>
        public const string Boolean = "boolean";
        /// <summary>Canonical hint for date-like values.</summary>
        public const string Date = "date";
        /// <summary>Canonical hint for values that should remain unshaped.</summary>
        public const string Raw = "raw";
        /// <summary>Canonical hint for array values.</summary>
        public const string Array = "array";

        /// <summary>
        /// Infers the canonical coercion hint for a CLR type.
        /// </summary>
        /// <param name="type">The CLR type to inspect.</param>
        /// <returns>The canonical coercion hint for the supplied type.</returns>
        public static string InferFromType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (underlying.IsArray) return Array;
            if (underlying.IsGenericType)
            {
                var genericType = underlying.GetGenericTypeDefinition();
                if (genericType == typeof(List<>) || genericType == typeof(IEnumerable<>) ||
                    genericType == typeof(ICollection<>) || genericType == typeof(IReadOnlyList<>))
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
        /// Infers the canonical coercion hint for the element type of a CLR collection type.
        /// </summary>
        /// <param name="type">The CLR collection type to inspect.</param>
        /// <returns>The canonical coercion hint for the collection element type.</returns>
        public static string InferElementType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            if (underlying.IsArray)
                return InferFromType(underlying.GetElementType()!);

            if (underlying.IsGenericType)
            {
                var args = underlying.GetGenericArguments();
                if (args.Length == 1)
                    return InferFromType(args[0]);
            }

            return Raw;
        }
    }
}
