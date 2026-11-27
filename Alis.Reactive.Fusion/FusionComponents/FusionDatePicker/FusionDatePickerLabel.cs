using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The position of a date editor’s floating prompt.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerLabel>))]
public enum FusionDatePickerLabel
{
    /// <summary>Keeps the entry hint inside the field.</summary>
    [JsonStringEnumMemberName("Never")]
    Never,

    /// <summary>Keeps the floating hint above the field.</summary>
    [JsonStringEnumMemberName("Always")]
    Always,

    /// <summary>Floats the hint when the field is focused or has a value.</summary>
    [JsonStringEnumMemberName("Auto")]
    Auto,
}
