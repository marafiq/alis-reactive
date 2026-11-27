using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The calendar system used to display the selected date.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerCalendar>))]
public enum FusionDatePickerCalendar
{
    /// <summary>Uses Gregorian months and years.</summary>
    [JsonStringEnumMemberName("Gregorian")]
    Gregorian,

    /// <summary>Uses Hijri months and years.</summary>
    [JsonStringEnumMemberName("Islamic")]
    Islamic,
}
