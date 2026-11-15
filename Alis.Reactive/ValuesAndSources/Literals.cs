using System;
using System.Globalization;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal static class Literals
{
    private const long LastWholeNumberJavaScriptHolds = 9007199254740991L;
    private const int DigitsEveryDoubleCarriesBack = 15;
    private const string NoJsonNumber = "NaN or an infinity, which no JSON number holds";
    private const string PastTheWholeNumbers = "a whole number past 9007199254740991";
    private const string PastTheDigits = "a decimal of more than 15 significant digits";

    internal static LitExpression Text(string? text) => new(JsonSerializer.SerializeToElement(text));

    internal static LitExpression Of<TValue>(TValue value, AppJson json) => new(JsonSerializer.SerializeToElement(value, json.LiveForm));

    internal static LitExpression Of(object? value, Type type, AppJson json) => new(JsonSerializer.SerializeToElement(value, type, json.LiveForm));

    internal static string Written<TValue>(TValue value, AppJson json)
    {
        if (JavaScriptReadsAnother(value, out _)) throw new NotImplementedException();
        return JsonSerializer.Serialize(value, json.LiveForm);
    }

    internal static Expression IntoLive<TValue>(TValue value, AppJson json)
    {
        if (JavaScriptReadsAnother(value, out _)) throw new NotImplementedException();
        var literal = Of(value, json);
        return LiveShapes.Converts(json.LiveForm.GetTypeInfo(typeof(TValue)), out var shape) ? new AsExpression(literal, shape) : literal;
    }

    internal static void RefuseWhereJavaScriptReadsAnother<TValue>(TValue value, string member)
    {
        if (!JavaScriptReadsAnother(value, out var reason)) return;
        throw new InvalidOperationException($"'{member}' holds a JavaScript number, and {Convert.ToString(value, CultureInfo.InvariantCulture)} is {reason}, which JavaScript reads as another value; write a finite number of at most 15 significant digits.");
    }

    private static bool JavaScriptReadsAnother<TValue>(TValue value, out string reason)
    {
        reason = value switch
        {
            double number when double.IsNaN(number) || double.IsInfinity(number) => NoJsonNumber,
            float number when float.IsNaN(number) || float.IsInfinity(number) => NoJsonNumber,
            decimal number when IsWhole(number) => number is < -LastWholeNumberJavaScriptHolds or > LastWholeNumberJavaScriptHolds ? PastTheWholeNumbers : string.Empty,
            decimal number when SignificantDigits(number) > DigitsEveryDoubleCarriesBack => PastTheDigits,
            long number when number is < -LastWholeNumberJavaScriptHolds or > LastWholeNumberJavaScriptHolds => PastTheWholeNumbers,
            ulong number when number > LastWholeNumberJavaScriptHolds => PastTheWholeNumbers,
            _ => string.Empty,
        };
        return reason.Length > 0;
    }

    private static bool IsWhole(decimal number) => decimal.Truncate(number) == number;

    private static int SignificantDigits(decimal number)
        => number.ToString(CultureInfo.InvariantCulture).Replace(".", "").TrimStart('-', '0').TrimEnd('0').Length;
}
