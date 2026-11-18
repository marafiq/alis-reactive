using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alis.Reactive.ValuesAndSources;

internal static class EnumRows
{
    private const decimal LastWholeNumberJavaScriptHolds = 9007199254740991m;

    private static readonly JsonStringEnumConverter EveryEnum = new();

    internal static bool IsEnum(Type type) => EveryEnum.CanConvert(EnumOf(type));

    internal static Type EnumOf(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    internal static bool IsOneTextNumberOrBoolean(JsonElement written)
        => written.ValueKind is JsonValueKind.String or JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False;

    internal static bool WrittenAlike(JsonElement one, JsonElement other) => one.ValueKind == other.ValueKind && KeyOf(one) == KeyOf(other);

    internal static List<HeldRow> Of(Type type, bool isFlags, string member, Func<Enum, string, JsonElement> held)
    {
        var rows = new List<HeldRow>();
        var byHeld = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Enum value in Enum.GetValues(EnumOf(type)))
        {
            var name = value.ToString();
            var written = held(value, name);
            var key = KeyOf(written);
            if (byHeld.TryGetValue(key, out var previous))
            {
                if (previous == name) continue;
                throw new InvalidOperationException($"'{member}' holds {previous} and {name} in one form, {written.GetRawText()}, under the app's options, so the browser cannot tell which one it holds; give one of them a JsonStringEnumMemberName of its own.");
            }

            byHeld.Add(key, name);
            rows.Add(new HeldRow(value, name, written, isFlags ? FlagNumber(value, name, member) : 0));
        }

        return rows;
    }

    private static string KeyOf(JsonElement written)
    {
        if (written.ValueKind != JsonValueKind.Number) return written.GetRawText();
        var number = written.GetDouble();
        return number == 0 ? "0" : number.ToString("R", CultureInfo.InvariantCulture);
    }

    private static long FlagNumber(Enum value, string name, string member)
    {
        var number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        if (number < 0 || number > LastWholeNumberJavaScriptHolds)
            throw new InvalidOperationException($"'{member}' names flags value {name} outside 0 to 9007199254740991; give each flag a non-negative JavaScript-safe integer.");
        return decimal.ToInt64(number);
    }

    internal sealed record HeldRow(Enum Value, string Name, JsonElement Held, long Number);
}
