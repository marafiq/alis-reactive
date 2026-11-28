using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Components;

[JsonConverter(typeof(JsonStringEnumConverter<Choice>))]
public enum Choice
{
    [JsonStringEnumMemberName("low")]
    Minimum,

    [JsonStringEnumMemberName("high")]
    Maximum,
}
