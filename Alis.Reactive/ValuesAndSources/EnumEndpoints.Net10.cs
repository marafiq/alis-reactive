#if NET10_0_OR_GREATER
using System;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;

namespace Alis.Reactive.ValuesAndSources;

internal static partial class EnumEndpoints
{
    internal static partial JsonElement Sent(Enum value, Type enumeration, AppJson json) => JsonSerializer.SerializeToElement(value, json.BodyForm.GetTypeInfo(enumeration));

    internal static partial JsonElement Literal<TValue>(TValue value, AppJson json) => JsonSerializer.SerializeToElement(value, json.BodyForm);

    internal static partial bool ReadsAsHeld(Enum value, JsonElement held, Type enumeration, AppJson json)
    {
        try
        {
            return Equals(held.Deserialize(json.App.GetTypeInfo(enumeration)), value);
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
#endif
