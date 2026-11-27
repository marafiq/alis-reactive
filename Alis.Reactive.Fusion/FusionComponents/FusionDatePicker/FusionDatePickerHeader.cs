using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The weekday heading style used in a calendar.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerHeader>))]
public enum FusionDatePickerHeader
{
    /// <summary>Uses short weekday names, such as Mo.</summary>
    [JsonStringEnumMemberName("Short")]
    Short,

    /// <summary>Uses the narrowest weekday labels, such as M.</summary>
    [JsonStringEnumMemberName("Narrow")]
    Narrow,

    /// <summary>Uses abbreviated weekday names, such as Mon.</summary>
    [JsonStringEnumMemberName("Abbreviated")]
    Abbreviated,

    /// <summary>Uses full weekday names.</summary>
    [JsonStringEnumMemberName("Wide")]
    Wide,
}
