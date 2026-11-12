using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class BindingStart
{
    public string Name { get; init; } = "";

    [JsonPropertyName("renamed_member")]
    public string Renamed { get; init; } = "";

    public string? Constructor { get; init; }
}
