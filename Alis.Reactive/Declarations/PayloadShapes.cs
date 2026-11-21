using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Declarations;

internal static class PayloadShapes
{
    private static readonly Type[] Fractions = [typeof(float), typeof(double), typeof(decimal)];
    private static readonly Type[] NullableIntegers = [typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?)];
    private static readonly Type[] NullableFractions = [typeof(float?), typeof(double?), typeof(decimal?)];

    internal static Shape? ReadAs(JsonTypeInfo member) => DateOf(member) ?? member switch
    {
        JsonTypeInfo<bool> => BooleanShape.Instance,
        JsonTypeInfo<bool?> => new NullableShape(BooleanShape.Instance),
        _ when Fractions.Contains(member.Type) => Number(NumberOf.Fraction, member),
        _ when NullableIntegers.Contains(member.Type) => new NullableShape(Number(NumberOf.Integer, member)),
        _ when NullableFractions.Contains(member.Type) => new NullableShape(Number(NumberOf.Fraction, member)),
        _ => null,
    };

    internal static Shape? DateOf(JsonTypeInfo member) => member switch
    {
        JsonTypeInfo<DateTime> => DateShape.Instance,
        JsonTypeInfo<DateTime?> => new NullableShape(DateShape.Instance),
        JsonTypeInfo<DateTimeOffset> => new InstantShape(DateMeaning.Instant),
        JsonTypeInfo<DateTimeOffset?> => new NullableShape(new InstantShape(DateMeaning.Instant)),
        _ => null,
    };

    internal static Shape? WrittenAsANumber(JsonElement start, JsonTypeInfo member)
        => member.Kind == JsonTypeInfoKind.None && start.ValueKind == JsonValueKind.Number ? Number(NumberOf.Integer, member) : null;

    private static NumberShape Number(NumberOf of, JsonTypeInfo member) => new(of, NumberCulture.Invariant, ConversionShapes.OverflowOf(member));
}
