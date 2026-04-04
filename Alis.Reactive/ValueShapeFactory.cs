using System;
using System.Collections;
using System.Collections.Generic;
using Alis.Reactive.PlanModel;

namespace Alis.Reactive
{
    internal static class ValueShapeFactory
    {
        internal static ValueShape String() => new ScalarValueShape("string");
        internal static ValueShape Number() => new ScalarValueShape("number");
        internal static ValueShape Boolean() => new ScalarValueShape("boolean");
        internal static ValueShape Date() => new ScalarValueShape("date");
        internal static ValueShape Raw() => new ScalarValueShape("raw");
        internal static ValueShape Any() => new AnyValueShape();

        internal static ValueShape FromToken(string token)
        {
            switch (token)
            {
                case "string":
                    return String();
                case "number":
                    return Number();
                case "boolean":
                    return Boolean();
                case "date":
                    return Date();
                case "raw":
                    return Raw();
                case "array":
                    return new ArrayValueShape(Any());
                default:
                    return Any();
            }
        }

        internal static ValueShape FromClrType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;

            if (TryGetSequenceItemType(underlying, out var itemType))
                return new ArrayValueShape(itemType == null ? Any() : FromClrType(itemType));

            if (underlying == typeof(string))
                return String();

            if (underlying == typeof(bool))
                return Boolean();

            if (underlying == typeof(int) || underlying == typeof(long) ||
                underlying == typeof(double) || underlying == typeof(float) ||
                underlying == typeof(decimal) || underlying == typeof(short) ||
                underlying == typeof(byte) || underlying == typeof(uint) ||
                underlying == typeof(ulong) || underlying == typeof(ushort) ||
                underlying == typeof(sbyte))
                return Number();

            if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset) ||
                underlying == typeof(DateOnly))
                return Date();

            if (underlying.IsEnum)
                return String();

            return Raw();
        }

        internal static ValueShape? ItemFromClrType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            if (!TryGetSequenceItemType(underlying, out var itemType) || itemType == null)
                return null;

            return FromClrType(itemType);
        }

        internal static ValueShape FromLiteral(object? value)
        {
            if (value == null)
                return Any();

            if (value is string)
                return String();

            if (value is bool)
                return Boolean();

            if (value is DateTime || value is DateTimeOffset || value is DateOnly)
                return Date();

            if (value is byte || value is sbyte || value is short || value is ushort ||
                value is int || value is uint || value is long || value is ulong ||
                value is float || value is double || value is decimal)
                return Number();

            if (!(value is string) && value is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                    return new ArrayValueShape(FromLiteral(item));

                return new ArrayValueShape(Any());
            }

            return Any();
        }

        internal static bool AreEquivalent(ValueShape? left, ValueShape? right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            if (left is ScalarValueShape leftScalar && right is ScalarValueShape rightScalar)
                return leftScalar.Type == rightScalar.Type;

            if (left is ArrayValueShape leftArray && right is ArrayValueShape rightArray)
                return AreEquivalent(leftArray.Item, rightArray.Item);

            if (left is ObjectValueShape leftObject && right is ObjectValueShape rightObject)
            {
                if (leftObject.Additional != rightObject.Additional)
                    return false;

                var leftFields = leftObject.Fields ?? new Dictionary<string, ValueShape>();
                var rightFields = rightObject.Fields ?? new Dictionary<string, ValueShape>();
                if (leftFields.Count != rightFields.Count)
                    return false;

                foreach (var field in leftFields)
                {
                    if (!rightFields.TryGetValue(field.Key, out var rightField))
                        return false;

                    if (!AreEquivalent(field.Value, rightField))
                        return false;
                }

                return true;
            }

            return left is AnyValueShape && right is AnyValueShape;
        }

        internal static string Describe(ValueShape? shape)
        {
            if (shape == null)
                return "null";

            if (shape is ScalarValueShape scalar)
                return scalar.Type;

            if (shape is ArrayValueShape array)
                return "array<" + Describe(array.Item) + ">";

            if (shape is ObjectValueShape)
                return "object";

            return "any";
        }

        private static bool TryGetSequenceItemType(Type type, out Type? itemType)
        {
            itemType = null;

            if (type.IsArray)
            {
                itemType = type.GetElementType();
                return true;
            }

            if (!type.IsGenericType)
                return false;

            var genericType = type.GetGenericTypeDefinition();
            if (genericType != typeof(List<>) && genericType != typeof(IEnumerable<>) &&
                genericType != typeof(ICollection<>) && genericType != typeof(IReadOnlyList<>))
                return false;

            var args = type.GetGenericArguments();
            if (args.Length != 1)
                return false;

            itemType = args[0];
            return true;
        }
    }
}
