using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class Starts
{
    [JsonObjectCreationHandling(JsonObjectCreationHandling.Populate)]
    public Spot Populated { get; set; } = new() { Place = "binding start" };

    public Spot GetterOnly { get; } = new() { Place = "binding start" };

    public Spot Replaced { get; set; } = new() { Place = "binding start" };
}
