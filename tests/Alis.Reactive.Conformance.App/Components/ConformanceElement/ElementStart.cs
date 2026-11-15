using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class ElementStart
{
    [JsonPropertyName("room")]
    public string Room { get; init; } = "";
}
