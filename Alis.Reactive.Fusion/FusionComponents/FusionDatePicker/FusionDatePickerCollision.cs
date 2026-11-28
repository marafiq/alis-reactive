using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The action when a calendar would extend beyond a viewport edge.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerCollision>))]
public enum FusionDatePickerCollision
{
    /// <summary>Preserves the requested position.</summary>
    [JsonStringEnumMemberName("none")]
    None,

    /// <summary>Places the calendar on the other side of the anchor.</summary>
    [JsonStringEnumMemberName("flip")]
    Flip,

    /// <summary>Fits the calendar inside the available viewport.</summary>
    [JsonStringEnumMemberName("fit")]
    Fit,
}
