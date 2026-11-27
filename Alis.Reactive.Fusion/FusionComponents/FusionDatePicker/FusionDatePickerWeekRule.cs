using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The convention used to number an admission week.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerWeekRule>))]
public enum FusionDatePickerWeekRule
{
    /// <summary>Numbers the week containing the first day of the year as week one.</summary>
    [JsonStringEnumMemberName("FirstDay")]
    FirstDay,

    /// <summary>Starts week one with the first full week of the year.</summary>
    [JsonStringEnumMemberName("FirstFullWeek")]
    FirstFullWeek,

    /// <summary>Starts week one with the first week containing at least four days of the year.</summary>
    [JsonStringEnumMemberName("FirstFourDayWeek")]
    FirstFourDayWeek,
}
