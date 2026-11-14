using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed record Binding(string Name, string Code) : BindingBase
{
    public int Count { get; init; }

    public string Started { get; init; } = "start";

    public StrictSpot Strict { get; init; } = new();

    public SealedSpot Sealed { get; init; } = null!;

    public PolySpot Polymorphic { get; init; } = new();

    public Shape Shaped { get; init; } = new();

    public string? Constructor { get; init; }

    [JsonConverter(typeof(ConvertedSpotAsItsName))]
    public ConvertedSpot Converted { get; init; } = new();

    [JsonPropertyName("renamed_member")]
    public override string Renamed { get; init; } = "";

    [JsonIgnore]
    [SuppressMessage("Alis.Reactive.Analyzers", "ALIS025", Justification = "The drift WhenAReadNamesAnOverrideTheJsonOptionsIgnore arranges: X18.8 refuses the read of it where the view renders")]
    public override string Ignored { get; init; } = "";
}
