using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.Schema;

internal static class PlanJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };
}
