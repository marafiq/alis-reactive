using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class SpotStart
{
    [JsonPropertyName("place")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Place { get; init; }
}
