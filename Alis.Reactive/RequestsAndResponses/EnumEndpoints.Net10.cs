#if NET10_0_OR_GREATER
using System;
using System.Text.Json;
using Alis.Reactive.PathsAndWire;

namespace Alis.Reactive.RequestsAndResponses;

internal static partial class EnumEndpoints
{
    internal static partial JsonElement Sent(Enum value, Type enumeration, AppJson json) => JsonSerializer.SerializeToElement(value, json.BodyForm.GetTypeInfo(enumeration));

    internal static partial JsonElement Literal<TValue>(TValue value, AppJson json) => JsonSerializer.SerializeToElement(value, json.BodyForm);
}
#endif
