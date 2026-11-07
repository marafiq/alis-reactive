using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Alis.Reactive.Plan;

internal static class PlanJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    };

    internal static string Write(PlanDocument document) => JsonSerializer.Serialize(document, Options);

    internal static string Name(string member) => Options.PropertyNamingPolicy!.ConvertName(member);
}
