using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class OuterStart
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = "";

    [JsonPropertyName("count")]
    public int Count { get; init; }
}
