using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// Where a <see cref="FusionButton"/> places its icon beside its text, written by its name.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<FusionButtonIconPosition>))]
public enum FusionButtonIconPosition
{
    /// <summary>Before the text.</summary>
    [JsonStringEnumMemberName("Left")]
    Left,

    /// <summary>After the text.</summary>
    [JsonStringEnumMemberName("Right")]
    Right,

    /// <summary>Above the text.</summary>
    [JsonStringEnumMemberName("Top")]
    Top,

    /// <summary>Below the text.</summary>
    [JsonStringEnumMemberName("Bottom")]
    Bottom,
}
