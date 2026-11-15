using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;
using Alis.Reactive.TypeKnowledge;

namespace Alis.Reactive.ValuesAndSources;

internal static class Literals
{
    private const long LastWholeNumberJavaScriptHolds = 9007199254740991L;
    private const int DigitsEveryDoubleCarriesBack = 15;
    private const string NoJsonNumber = "NaN or an infinity, which no JSON number holds";
    private const string PastTheWholeNumbers = "a whole number past 9007199254740991";
    private const string PastTheDigits = "a decimal of more than 15 significant digits";

    private static readonly ConditionalWeakTable<JsonSerializerOptions, JsonSerializerOptions> NonFiniteNamed = new();

    internal static LitExpression Text(string? text) => new(JsonSerializer.SerializeToElement(text));

    internal static LitExpression Of<TValue>(TValue value, AppJson json) => new(JsonSerializer.SerializeToElement(value, json.LiveForm));

    internal static LitExpression Of(object? value, Type type, AppJson json) => new(JsonSerializer.SerializeToElement(value, type, json.LiveForm));

    internal static string Written<TValue>(TValue value, AppJson json, string member)
    {
        RefuseAtAnyDepth(value, json.LiveForm, member);
        return JsonSerializer.Serialize(value, json.LiveForm);
    }

    internal static Expression IntoLive<TValue>(TValue value, AppJson json, string member)
    {
        RefuseAtAnyDepth(value, json.LiveForm, member);
        return Live(value, json);
    }

    internal static void RefuseWhereJavaScriptReadsAnother<TValue>(TValue value, string member)
    {
        var reason = WhyJavaScriptReadsAnother(value);
        if (reason.Length == 0) return;
        throw Refused(member, Convert.ToString(value, CultureInfo.InvariantCulture), reason);
    }

    internal static void RefuseAtAnyDepth<TValue>(TValue value, JsonSerializerOptions live, string member)
    {
        RefuseWhereJavaScriptReadsAnother(value, member);
        var declared = live.GetTypeInfo(typeof(TValue));
        if (declared.Kind == JsonTypeInfoKind.None) return;
        var written = JsonSerializer.SerializeToElement(value, typeof(TValue), NonFiniteNamed.GetValue(live, NamingNonFinite));
        RefuseWithin(written, declared, false, member);
    }

    private static void RefuseWithin(JsonElement written, JsonTypeInfo declared, bool writtenByItsOwn, string path)
    {
        if (declared.Kind == JsonTypeInfoKind.None) RefuseANumber(written, declared, writtenByItsOwn, path);
        if (declared.Kind == JsonTypeInfoKind.Object && written.ValueKind == JsonValueKind.Object) RefuseWithinMembers(written, declared.ContractItWasWrittenBy(written), path);
        if (declared.IsList(out var item) && written.ValueKind == JsonValueKind.Array) RefuseWithinItems(written, item, writtenByItsOwn, path);
        if (declared.IsDictionary(out _, out var entry) && written.ValueKind == JsonValueKind.Object) RefuseWithinEntries(written, entry, writtenByItsOwn, path);
    }

    private static void RefuseWithinMembers(JsonElement written, JsonTypeInfo declared, string path)
    {
        foreach (var property in declared.Properties)
        {
            if (!written.TryGetProperty(property.Name, out var held)) continue;
            var writtenByItsOwn = property.NumberHandling is not null || declared.NumberHandling is not null || property.CustomConverter is not null;
            RefuseWithin(held, declared.Options.GetTypeInfo(property.PropertyType), writtenByItsOwn, path + "." + property.Name);
        }
    }

    private static void RefuseWithinItems(JsonElement written, JsonTypeInfo item, bool writtenByItsOwn, string path)
    {
        for (var index = 0; index < written.GetArrayLength(); index++)
            RefuseWithin(written[index], item, writtenByItsOwn, path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]");
    }

    private static void RefuseWithinEntries(JsonElement written, JsonTypeInfo entry, bool writtenByItsOwn, string path)
    {
        foreach (var held in written.EnumerateObject())
            RefuseWithin(held.Value, entry, writtenByItsOwn, path + "[\"" + held.Name + "\"]");
    }

    private static void RefuseANumber(JsonElement written, JsonTypeInfo declared, bool writtenByItsOwn, string path)
    {
        var reason = written.ValueKind switch
        {
            JsonValueKind.Number => WhyJavaScriptReadsAnother(written, declared),
            JsonValueKind.String when !writtenByItsOwn && declared is JsonTypeInfo<double> or JsonTypeInfo<double?> or JsonTypeInfo<float> or JsonTypeInfo<float?> => NoJsonNumber,
            _ => string.Empty,
        };
        if (reason.Length == 0) return;
        throw Refused(path, written.ValueKind == JsonValueKind.String ? written.GetString() : written.GetRawText(), reason);
    }

    private static string WhyJavaScriptReadsAnother(JsonElement written, JsonTypeInfo declared) => declared switch
    {
        JsonTypeInfo<decimal> or JsonTypeInfo<decimal?> => WhyJavaScriptReadsAnother(written.GetDecimal()),
        JsonTypeInfo<long> or JsonTypeInfo<long?> => WhyJavaScriptReadsAnother(written.GetInt64()),
        JsonTypeInfo<ulong> or JsonTypeInfo<ulong?> => WhyJavaScriptReadsAnother(written.GetUInt64()),
        _ => string.Empty,
    };

    private static InvalidOperationException Refused(string member, string? value, string reason)
        => new($"'{member}' holds a JavaScript number, and {value} is {reason}, which JavaScript reads as another value; write a finite number of at most 15 significant digits.");

    private static JsonSerializerOptions NamingNonFinite(JsonSerializerOptions live) => new(live) { NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };

    private static string WhyJavaScriptReadsAnother<TValue>(TValue value)
        => value switch
        {
            double number when double.IsNaN(number) || double.IsInfinity(number) => NoJsonNumber,
            float number when float.IsNaN(number) || float.IsInfinity(number) => NoJsonNumber,
            decimal number when IsWhole(number) => number is < -LastWholeNumberJavaScriptHolds or > LastWholeNumberJavaScriptHolds ? PastTheWholeNumbers : string.Empty,
            decimal number when SignificantDigits(number) > DigitsEveryDoubleCarriesBack => PastTheDigits,
            long number when number is < -LastWholeNumberJavaScriptHolds or > LastWholeNumberJavaScriptHolds => PastTheWholeNumbers,
            ulong number when number > LastWholeNumberJavaScriptHolds => PastTheWholeNumbers,
            _ => string.Empty,
        };

    private static Expression Live<TValue>(TValue value, AppJson json)
    {
        var literal = Of(value, json);
        return LiveShapes.Converts(json.LiveForm.GetTypeInfo(typeof(TValue)), out var shape) ? new AsExpression(literal, shape) : literal;
    }

    private static bool IsWhole(decimal number) => decimal.Truncate(number) == number;

    private static int SignificantDigits(decimal number)
        => number.ToString(CultureInfo.InvariantCulture).Replace(".", "").TrimStart('-', '0').TrimEnd('0').Length;
}
