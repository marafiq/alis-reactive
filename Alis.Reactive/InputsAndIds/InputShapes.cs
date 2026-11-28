using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;
using Alis.Reactive.ValuesAndSources;

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

    internal static Shape Of(JsonTypeInfo member, JsonElement start, string input, AppJson json) => Of(member, start, NumberCulture.View, input, json);

    internal static Shape Of(JsonTypeInfo member, JsonElement start, NumberCulture numbers, string input, AppJson json) => member switch
    {
        JsonTypeInfo<string> or JsonTypeInfo<char> or JsonTypeInfo<char?> => new StringShape(DateMeaning.None, "0"),
        JsonTypeInfo<bool> or JsonTypeInfo<bool?> => BooleanShape.Instance,
        JsonTypeInfo<DateTime> or JsonTypeInfo<DateTime?> => DateShape.Instance,
        JsonTypeInfo<DateTimeOffset> or JsonTypeInfo<DateTimeOffset?> => new InstantShape(DateMeaning.None),
        _ when IsAHostsFile(member) => FileShape.Instance,
        _ when EnumRows.IsEnum(member.Type) => LiveRows(member, input, json),
        _ when Integers.Contains(member.Type) => new NumberShape(NumberOf.Integer, numbers, ConversionShapes.OverflowOf(member)),
        _ when Fractions.Contains(member.Type) => new NumberShape(NumberOf.Fraction, numbers, ConversionShapes.OverflowOf(member)),
        _ when member.IsList(out var item) && HoldsOneValue(item) => new ArrayShape(Nullable.GetUnderlyingType(item.Type) is null ? Of(item, NoStart, numbers, input, json) : new NullableShape(Of(item, NoStart, numbers, input, json))),
        _ when member.Kind == JsonTypeInfoKind.None => WrittenAs(start, numbers),
        _ => throw SentAsMoreThanOneText(input, member),
    };

    internal static Shape From(Shape shape, StringShape source) => shape switch
    {
        StringShape => source,
        InstantShape => new InstantShape(source.Date),
        ArrayShape array => new ArrayShape(From(array.Item, source)),
        NullableShape nullable => new NullableShape(From(nullable.Inner, source)),
        _ => shape,
    };

    internal static bool HoldsAnEnum(Shape shape) => shape is EnumShape or ArrayShape { Item: EnumShape or NullableShape { Inner: EnumShape } };

    private static partial bool IsAHostsFile(JsonTypeInfo member);

    private static EnumShape LiveRows(JsonTypeInfo member, string input, AppJson json)
    {
        var enumeration = EnumRows.EnumOf(member.Type);
        var isFlags = json.NamesFlags(enumeration);
        var live = member.Options.GetTypeInfo(enumeration);
        var rows = EnumRows.Of(member.Type, isFlags, input, (value, _) => JsonSerializer.SerializeToElement(value, live)).Select(row => new LiveEnumValue(row.Held, row.Held, row.Number));
        return new EnumShape(isFlags, isFlags ? [.. rows.OrderByDescending(row => row.Number)] : [.. rows]);
    }

    private static bool HoldsOneValue(JsonTypeInfo item) => item.Kind == JsonTypeInfoKind.None || IsAHostsFile(item);

    private static InvalidOperationException SentAsMoreThanOneText(string input, JsonTypeInfo member)
        => new($"Input '{input}' binds {ClassNames.AsCSharpWritesIt(member.Type)}, which the app's JSON options write as {WrittenForm(member)}, and an input binds a member the browser sends as one text, a list of texts or files: bind an input to each value it holds, render its view with Html.PartialFor, or Set it in a JSON body from the component that holds it.");

    private static string WrittenForm(JsonTypeInfo member) => member switch
    {
        _ when member.IsList(out var item) => "a list of " + ItemsWrittenForm(item),
        { Kind: JsonTypeInfoKind.Dictionary } => "a dictionary",
        _ => "an object",
    };

    private static string ItemsWrittenForm(JsonTypeInfo item) => item.Kind switch
    {
        JsonTypeInfoKind.Enumerable => "lists",
        JsonTypeInfoKind.Dictionary => "dictionaries",
        _ => "objects",
    };

    private static Shape WrittenAs(JsonElement start, NumberCulture numbers) => start.ValueKind switch
    {
        JsonValueKind.Number => new NumberShape(NumberOf.Integer, numbers, NumberOverflow.Nothing),
        _ => new StringShape(DateMeaning.None, "0"),
    };
}
