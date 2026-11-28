using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ItemStart
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";

    [JsonPropertyName("active")]
    public bool Active { get; init; }

    [JsonPropertyName("value")]
    public int Value { get; init; }

    [JsonPropertyName("optionalCount")]
    public int? OptionalCount { get; init; }

    [JsonPropertyName("optionalScore")]
    public int? OptionalScore { get; init; }

    [JsonPropertyName("lacksName")]
    public bool LacksName { get; init; }

    [JsonPropertyName("lacksActive")]
    public bool LacksActive { get; init; }
}
