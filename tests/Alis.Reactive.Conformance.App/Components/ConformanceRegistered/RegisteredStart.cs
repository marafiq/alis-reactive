using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class RegisteredStart
{
    [JsonPropertyName("reading")]
    public int Reading { get; init; }

    [JsonPropertyName("madeBy")]
    public string? MadeBy { get; init; }
}
