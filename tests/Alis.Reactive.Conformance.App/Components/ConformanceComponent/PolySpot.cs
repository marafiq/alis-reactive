using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

[JsonPolymorphic]
[JsonDerivedType(typeof(NamedPolySpot), "named")]
public abstract class PolySpot
{
    public string Name { get; set; } = "";
}
