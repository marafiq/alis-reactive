using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>Where a <see cref="FusionCheckBox"/> places its caption relative to its mark.</summary>
[JsonConverter(typeof(FusionCheckBoxLabelPositionConverter))]
public enum FusionCheckBoxLabelPosition
{
    /// <summary>The mark comes before its caption.</summary>
    [JsonStringEnumMemberName("After")]
    After,

    /// <summary>The caption comes before the mark.</summary>
    [JsonStringEnumMemberName("Before")]
    Before,
}
