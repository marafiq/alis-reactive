using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.TypeKnowledge;

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

        if (isFlags) RefuseSharedBits(rows, type, member);
        return rows;
    }

    private static void RefuseSharedBits(List<HeldRow> rows, Type type, string member)
    {
        for (var first = 0; first < rows.Count; first++)
        {
            for (var second = first + 1; second < rows.Count; second++)
            {
                var left = rows[first];
                var right = rows[second];
                var shared = left.Number & right.Number;
                if (shared == 0 || shared == left.Number || shared == right.Number) continue;
                throw new InvalidOperationException($"'{member}' holds {ClassNames.AsCSharpWritesIt(EnumOf(type))}, whose values {left.Name} ({left.Number.ToString(CultureInfo.InvariantCulture)}) and {right.Name} ({right.Number.ToString(CultureInfo.InvariantCulture)}) share a bit and neither holds the other; the framework splits a combination only over values that are disjoint or one inside the other, where its split, FluentValidation's IsInEnum and net10's binding agree on every value; give each value bits no other value shares, or make one hold the other.");
            }
        }
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
