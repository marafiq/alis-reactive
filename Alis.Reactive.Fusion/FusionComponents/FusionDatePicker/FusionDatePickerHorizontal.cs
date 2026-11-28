using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The horizontal alignment of a date calendar to its anchor.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerHorizontal>))]
public enum FusionDatePickerHorizontal
{
    /// <summary>Aligns at the left of the target.</summary>
    [JsonStringEnumMemberName("left")]
    Left,

    /// <summary>Aligns at the horizontal center of the target.</summary>
    [JsonStringEnumMemberName("center")]
    Center,

    /// <summary>Aligns at the right of the target.</summary>
    [JsonStringEnumMemberName("right")]
    Right,
}
