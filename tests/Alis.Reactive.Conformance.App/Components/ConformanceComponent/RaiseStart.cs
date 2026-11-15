using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class RaiseStart
{
    [JsonPropertyName("button")]
    public string Button { get; init; } = "";

    [JsonPropertyName("nullableText")]
    public string? NullableText { get; init; }

    [JsonPropertyName("optionalText")]
    public string? OptionalText { get; init; }

    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("noArgument")]
    public bool NoArgument { get; init; }

    [JsonPropertyName("flag")]
    public bool Flag { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [JsonPropertyName("emptyRecord")]
    public bool EmptyRecord { get; init; }

    [JsonPropertyName("record")]
    public RecordStart? Record { get; init; }
}
