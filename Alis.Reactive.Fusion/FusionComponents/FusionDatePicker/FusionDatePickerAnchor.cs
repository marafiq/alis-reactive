using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The reference used when positioning a date calendar at its anchor.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerAnchor>))]
public enum FusionDatePickerAnchor
{
    /// <summary>Uses the requested point on the target as the popup origin.</summary>
    [JsonStringEnumMemberName("relative")]
    Relative,

    /// <summary>Aligns the popup within the target bounds.</summary>
    [JsonStringEnumMemberName("container")]
    Container,
}
