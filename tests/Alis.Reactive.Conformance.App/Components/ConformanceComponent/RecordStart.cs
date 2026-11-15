using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class RecordStart
{
    [JsonPropertyName("name")]
    public string? Name { get; init; } = "";

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("day")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Day { get; init; }

    [JsonPropertyName("detail")]
    public DetailStart Detail { get; init; } = new();

    [JsonPropertyName("multiWord")]
    public int MultiWord { get; init; }
}
