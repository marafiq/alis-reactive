using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.PathsAndWire;

internal static class LiveShapes
{
    private static readonly Type[] Integers = [typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong)];
    private static readonly Type[] Fractions = [typeof(float), typeof(double), typeof(decimal)];
    private static readonly Type[] NullableIntegers = [typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?)];
    private static readonly Type[] NullableFractions = [typeof(float?), typeof(double?), typeof(decimal?)];

    internal static bool Converts(JsonTypeInfo type, [NotNullWhen(true)] out Shape? shape)
    {
        shape = new Walk().Of(type, type.Options.NumberHandling);
        return shape is not null;
    }

    private static Shape? Scalar(JsonTypeInfo type, JsonNumberHandling handling) => type switch
    {
        JsonTypeInfo<DateTime> => DateShape.Instance,
        JsonTypeInfo<DateTime?> => new NullableShape(DateShape.Instance),
        JsonTypeInfo<DateTimeOffset> => InstantShape.Instance,
        JsonTypeInfo<DateTimeOffset?> => new NullableShape(InstantShape.Instance),
        _ when (handling & JsonNumberHandling.WriteAsString) == 0 => null,
        _ when Integers.Contains(type.Type) => new NumberShape(NumberOf.Integer, NumberCulture.Invariant),
        _ when Fractions.Contains(type.Type) => new NumberShape(NumberOf.Fraction, NumberCulture.Invariant),
        _ when NullableIntegers.Contains(type.Type) => new NullableShape(new NumberShape(NumberOf.Integer, NumberCulture.Invariant)),
        _ when NullableFractions.Contains(type.Type) => new NullableShape(new NumberShape(NumberOf.Fraction, NumberCulture.Invariant)),
        _ => null,
    };

    private static bool HoldsADate(Shape shape) => shape switch
    {
        DateShape or InstantShape => true,
        NullableShape nullable => HoldsADate(nullable.Inner),
        ArrayShape array => HoldsADate(array.Item),
        ObjectShape holder => holder.Fields.Values.Any(HoldsADate),
        _ => false,
    };

    private sealed class Walk
    {
        private readonly HashSet<JsonTypeInfo> _path = [];
        private readonly HashSet<JsonTypeInfo> _heldAgain = [];

        internal Shape? Of(JsonTypeInfo type, JsonNumberHandling handling)
        {
            if (Scalar(type, handling) is { } scalar) return scalar;
            if (!_path.Add(type))
            {
                _heldAgain.Add(type);
                return null;
            }

            try
            {
                var shape = type.Kind switch
                {
                    JsonTypeInfoKind.Enumerable => List(type, handling),
                    JsonTypeInfoKind.Object => Object(type),
                    JsonTypeInfoKind.Dictionary => Dictionary(type, handling),
                    _ => null,
                };
                if (shape is not null && _heldAgain.Contains(type))
                {
                    throw new InvalidOperationException(
                        "A value written into a live member holds its own type and a date in it. A date reaches a live member through a shape that ends: pass the dates as a list of their own.");
                }

                return shape;
            }
            finally
            {
                _path.Remove(type);
            }
        }

        private ArrayShape? List(JsonTypeInfo type, JsonNumberHandling handling)
            => type.IsList(out var item) && Of(item, handling) is { } itemShape ? new ArrayShape(itemShape) : null;

        private ObjectShape? Object(JsonTypeInfo type)
        {
            var fields = new Dictionary<string, Shape>(StringComparer.Ordinal);
            foreach (var property in type.Properties.Where(property => property.Get is not null))
            {
                var handling = property.NumberHandling ?? type.NumberHandling ?? type.Options.NumberHandling;
                if (Of(type.Options.GetTypeInfo(property.PropertyType), handling) is { } field) fields[property.Name] = field;
            }

            return fields.Count == 0 ? null : new ObjectShape(fields);
        }

        private Shape? Dictionary(JsonTypeInfo type, JsonNumberHandling handling)
        {
            var holdsDates = type.IsDictionary(out _, out var value) && Of(value, handling) is { } valueShape && HoldsADate(valueShape);
            if (!holdsDates) return null;
            throw new InvalidOperationException(
                "A value written into a live member is a dictionary whose values hold dates. A date reaches a live member by its member's name: pass the entries as a list of objects that name each key.");
        }
    }
}
