using System;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;

namespace Alis.Reactive.ValuesAndSources;

internal static partial class EnumEndpoints
{
    internal static partial JsonElement Sent(Enum value, Type enumeration, AppJson json);

    internal static partial JsonElement Literal<TValue>(TValue value, AppJson json);

    internal static partial bool ReadsAsHeld(Enum value, JsonElement held, Type enumeration, AppJson json);
}
