using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class CallbacksStart
{
    [JsonPropertyName("own")]
    public string Own { get; init; } = "own";

    [JsonPropertyName("ownNamesGeneration")]
    public bool OwnNamesGeneration { get; init; }

    [JsonPropertyName("raise")]
    public string Raise { get; init; } = "Raise room 7";

    [JsonPropertyName("freeze")]
    public string Freeze { get; init; } = "Freeze";
}
