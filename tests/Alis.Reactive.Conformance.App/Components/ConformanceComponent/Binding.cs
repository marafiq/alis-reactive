using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed record Binding(string Name, string Code) : BindingBase
{
    public int Count { get; init; }

    public string Started { get; init; } = "start";

    public StrictSpot Strict { get; init; } = new();

    public SealedSpot? Sealed { get; init; }

    public PolySpot Polymorphic { get; init; } = new();

    public Shape Shaped { get; init; } = new();

    public string? Constructor { get; init; }

    public ConvertedSpot Converted { get; init; } = new();

    [JsonPropertyName("renamed_member")]
    public override string Renamed { get; init; } = "";

    [JsonIgnore]
    public override string Ignored { get; init; } = "";
}
