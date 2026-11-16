using System;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;

namespace Alis.Reactive.RequestsAndResponses;

internal static partial class EnumEndpoints
{
    internal static partial JsonElement Sent(Enum value, Type enumeration, AppJson json);

    internal static partial JsonElement Literal<TValue>(TValue value, AppJson json);
}
