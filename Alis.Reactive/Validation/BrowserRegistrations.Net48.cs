#if NET48
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Validation;

internal static partial class BrowserRegistrations
{
    private static partial BoundObjectJson ObjectJson(JsonPropertyInfo property, JsonTypeInfo holder) => BoundObjectJson.Fill;

    private static partial BoundListJson ListJson(JsonPropertyInfo property, JsonTypeInfo holder) => BoundListJson.Replace;
}
#endif
