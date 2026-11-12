using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

public enum Choice
{
    [JsonStringEnumMemberName("low")]
    Minimum,

    [JsonStringEnumMemberName("high")]
    Maximum,
}
