using System;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal static class Literals
{
    internal static LitExpression Text(string? text) => new(JsonSerializer.SerializeToElement(text));

    internal static LitExpression Of<TValue>(TValue value, AppJson json) => new(JsonSerializer.SerializeToElement(value, json.LiveForm));

    internal static LitExpression Of(object? value, Type type, AppJson json) => new(JsonSerializer.SerializeToElement(value, type, json.LiveForm));

    internal static string Written<TValue>(TValue value, AppJson json) => JsonSerializer.Serialize(value, json.LiveForm);

    internal static Expression IntoLive<TValue>(TValue value, AppJson json)
    {
        var literal = Of(value, json);
        return LiveShapes.Converts(json.LiveForm.GetTypeInfo(typeof(TValue)), out var shape) ? new AsExpression(literal, shape) : literal;
    }
}
