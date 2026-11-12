using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

[JsonPolymorphic]
public sealed class PolySpot
{
    public string Name { get; set; } = "";
}
