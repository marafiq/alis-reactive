using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class HeldStart
{
    [JsonPropertyName("items")]
    public IReadOnlyList<ItemStart> Items { get; init; } = [];

    [JsonPropertyName("total")]
    public int Total { get; init; }

    [JsonPropertyName("matchingItems")]
    public int MatchingItems { get; init; }
}
