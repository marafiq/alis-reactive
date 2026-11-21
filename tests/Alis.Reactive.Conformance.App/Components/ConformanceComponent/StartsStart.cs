using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class StartsStart
{
    [JsonPropertyName("populated")]
    public SpotStart Populated { get; init; } = new();

    [JsonPropertyName("getterOnly")]
    public SpotStart GetterOnly { get; init; } = new();

    [JsonPropertyName("replaced")]
    public SpotStart Replaced { get; init; } = new();
}
