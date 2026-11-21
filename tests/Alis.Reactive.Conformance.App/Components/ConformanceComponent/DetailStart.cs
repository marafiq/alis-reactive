using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class DetailStart
{
    [JsonPropertyName("name")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }
}
