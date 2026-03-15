using System;

namespace Alis.Reactive.Descriptors
{
    public static class CoercionTypes
    {
        public const string String = "string";
        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string Date = "date";
        public const string Raw = "raw";

        public static string InferFromType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
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
    }
}
