using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.InputsAndIds;

internal static partial class InputShapes
{
    private static readonly Type[] Integers =
    [
        typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
        typeof(byte?), typeof(sbyte?), typeof(short?), typeof(ushort?), typeof(int?), typeof(uint?), typeof(long?), typeof(ulong?),
    ];

    private static readonly Type[] Fractions = [typeof(float), typeof(double), typeof(decimal), typeof(float?), typeof(double?), typeof(decimal?)];

    private static readonly JsonElement NoStart = default;

    internal static Shape Of(JsonTypeInfo member, JsonElement start) => Of(member, start, NumberCulture.View);

    internal static Shape Of(JsonTypeInfo member, JsonElement start, NumberCulture numbers) => member switch
    {
        JsonTypeInfo<string> or JsonTypeInfo<char> or JsonTypeInfo<char?> => new StringShape(DateMeaning.None, "0"),
        JsonTypeInfo<bool> or JsonTypeInfo<bool?> => BooleanShape.Instance,
        JsonTypeInfo<DateTime> or JsonTypeInfo<DateTime?> => DateShape.Instance,
        JsonTypeInfo<DateTimeOffset> or JsonTypeInfo<DateTimeOffset?> => new InstantShape(DateMeaning.None),
        _ when IsAHostsFile(member) => FileShape.Instance,
        _ when Integers.Contains(member.Type) => new NumberShape(NumberOf.Integer, numbers, ConversionShapes.OverflowOf(member)),
        _ when Fractions.Contains(member.Type) => new NumberShape(NumberOf.Fraction, numbers, ConversionShapes.OverflowOf(member)),
        _ when member.IsList(out var item) => new ArrayShape(Nullable.GetUnderlyingType(item.Type) is null ? Of(item, NoStart, numbers) : new NullableShape(Of(item, NoStart, numbers))),
        _ when member.Kind == JsonTypeInfoKind.None => WrittenAs(start, numbers),
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

    private static partial bool IsAHostsFile(JsonTypeInfo member);

    private static Shape WrittenAs(JsonElement start, NumberCulture numbers) => start.ValueKind switch
    {
        JsonValueKind.Number => new NumberShape(NumberOf.Integer, numbers, NumberOverflow.Nothing),
        _ => new StringShape(DateMeaning.None, "0"),
    };
}
