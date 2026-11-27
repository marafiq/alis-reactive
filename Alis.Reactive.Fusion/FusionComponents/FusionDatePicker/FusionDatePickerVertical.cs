using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The vertical alignment of a date calendar to its anchor.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerVertical>))]
public enum FusionDatePickerVertical
{
    /// <summary>Aligns at the top of the target.</summary>
    [JsonStringEnumMemberName("top")]
    Top,

    /// <summary>Aligns at the vertical center of the target.</summary>
    [JsonStringEnumMemberName("center")]
    Center,

    /// <summary>Aligns at the bottom of the target.</summary>
    [JsonStringEnumMemberName("bottom")]
    Bottom,
}
