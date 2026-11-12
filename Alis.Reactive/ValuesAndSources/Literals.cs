using System;
using System.Text.Json;
using Alis.Reactive.Schema;

namespace Alis.Reactive.ValuesAndSources;

internal static class Literals
{
    internal static LitExpression Of<TValue>(TValue value)
    {
        var isAText = typeof(TValue) == typeof(string);
        if (!isAText) throw new NotImplementedException();
        return new LitExpression(JsonSerializer.SerializeToElement(value));
    }
}
