using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>Where a radio places its caption relative to its control.</summary>
[JsonConverter(typeof(FusionRadioButtonLabelPositionConverter))]
public enum FusionRadioButtonLabelPosition
{
    /// <summary>After the control.</summary>
    [JsonStringEnumMemberName("After")]
    After,

    /// <summary>Before the control.</summary>
    [JsonStringEnumMemberName("Before")]
    Before,
}
