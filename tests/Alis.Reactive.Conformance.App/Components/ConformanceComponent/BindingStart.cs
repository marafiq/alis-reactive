using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class BindingStart
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("renamed_member")]
    public string Renamed { get; init; } = "";

    [JsonPropertyName("constructor")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Constructor { get; init; }
}
