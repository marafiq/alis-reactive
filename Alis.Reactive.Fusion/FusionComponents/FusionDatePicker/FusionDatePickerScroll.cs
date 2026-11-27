using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The action of a date calendar popup when its register scrolls.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerScroll>))]
public enum FusionDatePickerScroll
{
    /// <summary>Keeps the popup aligned with its target as a parent scrolls.</summary>
    [JsonStringEnumMemberName("reposition")]
    Reposition,

    /// <summary>Closes the popup when a scrollable ancestor moves.</summary>
    [JsonStringEnumMemberName("hide")]
    Hide,

    /// <summary>Keeps the popup in its current position during a scroll.</summary>
    [JsonStringEnumMemberName("none")]
    None,
}
