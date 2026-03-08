using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    public static class GuardOp
    {
        public const string Eq = "eq";
        public const string Neq = "neq";
        public const string Gt = "gt";
        public const string Gte = "gte";
        public const string Lt = "lt";
        public const string Lte = "lte";
        public const string Truthy = "truthy";
        public const string Falsy = "falsy";
        public const string IsNull = "is-null";
        public const string NotNull = "not-null";
    }

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

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(ValueGuard), "value")]
    [JsonDerivedType(typeof(AllGuard), "all")]
    [JsonDerivedType(typeof(AnyGuard), "any")]
    public abstract class Guard { }

    public sealed class ValueGuard : Guard
    {
        public string Source { get; }
        public string CoerceAs { get; }
        public string Op { get; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Operand { get; }

        public ValueGuard(string source, string coerceAs, string op, object? operand = null)
        {
            Source = source;
            CoerceAs = coerceAs;
            Op = op;
            Operand = operand;
        }
    }

    public sealed class AllGuard : Guard
    {
        public IReadOnlyList<Guard> Guards { get; }

        public AllGuard(IReadOnlyList<Guard> guards)
        {
            if (guards == null || guards.Count == 0)
                throw new ArgumentException("AllGuard requires at least one guard.", nameof(guards));
            Guards = guards;
        }
    }

    public sealed class AnyGuard : Guard
    {
        public IReadOnlyList<Guard> Guards { get; }

        public AnyGuard(IReadOnlyList<Guard> guards)
        {
            if (guards == null || guards.Count == 0)
                throw new ArgumentException("AnyGuard requires at least one guard.", nameof(guards));
            Guards = guards;
        }
    }
}
