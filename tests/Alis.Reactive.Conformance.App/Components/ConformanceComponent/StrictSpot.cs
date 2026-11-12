using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public sealed class StrictSpot
{
    [JsonRequired]
    public string Place { get; set; } = "";

    public string Name { get; set; } = "";
}
