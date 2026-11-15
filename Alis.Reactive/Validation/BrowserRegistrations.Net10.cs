#if NET10_0_OR_GREATER
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alis.Reactive.Schema;

namespace Alis.Reactive.Validation;

internal static partial class BrowserRegistrations
{
    private static partial BoundObjectJson ObjectJson(JsonPropertyInfo property, JsonTypeInfo holder)
        => Populates(property, holder) ? BoundObjectJson.Fill : BoundObjectJson.Replace;

    private static partial BoundListJson ListJson(JsonPropertyInfo property, JsonTypeInfo holder)
        => Populates(property, holder) ? BoundListJson.Append : BoundListJson.Replace;

    private static bool Populates(JsonPropertyInfo property, JsonTypeInfo holder)
        => (property.ObjectCreationHandling ?? holder.PreferredPropertyObjectCreationHandling ?? holder.Options.PreferredObjectCreationHandling) == JsonObjectCreationHandling.Populate;
}
#endif
