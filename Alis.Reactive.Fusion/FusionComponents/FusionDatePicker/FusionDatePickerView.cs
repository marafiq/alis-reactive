using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The calendar level used when selecting an arrival date.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerView>))]
public enum FusionDatePickerView
{
    /// <summary>Displays days in one month.</summary>
    [JsonStringEnumMemberName("Month")]
    Month,

    /// <summary>Displays months in one year.</summary>
    [JsonStringEnumMemberName("Year")]
    Year,

    /// <summary>Displays years in a decade.</summary>
    [JsonStringEnumMemberName("Decade")]
    Decade,
}
