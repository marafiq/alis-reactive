using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.InputsAndIds;

internal static class InputShapes
{
    private static readonly Type[] Integers =
    [
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?),
    ];

    private static readonly Type[] Fractions = [typeof(float), typeof(double), typeof(decimal), typeof(float?), typeof(double?), typeof(decimal?)];

    private static readonly JsonElement NoStart = default;

    internal static Shape Of(JsonTypeInfo member, JsonElement start) => member switch
    {
        JsonTypeInfo<string> or JsonTypeInfo<char> or JsonTypeInfo<char?> => new StringShape(DateMeaning.None, "0"),
        JsonTypeInfo<bool> or JsonTypeInfo<bool?> => BooleanShape.Instance,
        JsonTypeInfo<DateTime> or JsonTypeInfo<DateTime?> => DateShape.Instance,
        JsonTypeInfo<DateTimeOffset> or JsonTypeInfo<DateTimeOffset?> => new InstantShape(DateMeaning.None),
        _ when Integers.Contains(member.Type) => new NumberShape(NumberOf.Integer, NumberCulture.View, ConversionShapes.OverflowOf(member)),
        _ when Fractions.Contains(member.Type) => new NumberShape(NumberOf.Fraction, NumberCulture.View, ConversionShapes.OverflowOf(member)),
        _ when member.IsList(out var item) => new ArrayShape(Nullable.GetUnderlyingType(item.Type) is null ? Of(item, NoStart) : new NullableShape(Of(item, NoStart))),
        _ when member.Kind == JsonTypeInfoKind.None => WrittenAs(start),
        _ => AnyShape.Instance,
    };

    internal static Shape From(Shape shape, StringShape source) => shape switch
    {
        StringShape => source,
        InstantShape => new InstantShape(source.Date),
        ArrayShape array => new ArrayShape(From(array.Item, source)),
        NullableShape nullable => new NullableShape(From(nullable.Inner, source)),
        _ => shape,
    };

    private static Shape WrittenAs(JsonElement start) => start.ValueKind switch
    {
        JsonValueKind.Number => new NumberShape(NumberOf.Integer, NumberCulture.View, NumberOverflow.Nothing),
        _ => new StringShape(DateMeaning.None, "0"),
    };
}
