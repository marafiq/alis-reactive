using System.Text.Json.Serialization;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>The keyboard actions recognized by the date editor.</summary>
[JsonConverter(typeof(FusionDatePickerEnumConverter<FusionDatePickerKey>))]
public enum FusionDatePickerKey
{
    /// <summary>Closes the open date calendar.</summary>
    [JsonStringEnumMemberName("altUpArrow")]
    AltUpArrow,

    /// <summary>Opens the date calendar.</summary>
    [JsonStringEnumMemberName("altDownArrow")]
    AltDownArrow,

    /// <summary>Dismisses the calendar.</summary>
    [JsonStringEnumMemberName("escape")]
    Escape,

    /// <summary>Commits typed entry in the date field.</summary>
    [JsonStringEnumMemberName("enter")]
    Enter,

    /// <summary>Moves to a broader calendar view.</summary>
    [JsonStringEnumMemberName("controlUp")]
    ControlUp,

    /// <summary>Moves to a more detailed calendar view.</summary>
    [JsonStringEnumMemberName("controlDown")]
    ControlDown,

    /// <summary>Moves to the next row of the calendar.</summary>
    [JsonStringEnumMemberName("moveDown")]
    MoveDown,

    /// <summary>Moves to the preceding row of the calendar.</summary>
    [JsonStringEnumMemberName("moveUp")]
    MoveUp,

    /// <summary>Moves to the preceding calendar cell.</summary>
    [JsonStringEnumMemberName("moveLeft")]
    MoveLeft,

    /// <summary>Moves to the next calendar cell.</summary>
    [JsonStringEnumMemberName("moveRight")]
    MoveRight,

    /// <summary>Selects the focused calendar item.</summary>
    [JsonStringEnumMemberName("select")]
    Select,

    /// <summary>Moves to the start of the displayed period.</summary>
    [JsonStringEnumMemberName("home")]
    Home,

    /// <summary>Moves to the end of the displayed period.</summary>
    [JsonStringEnumMemberName("end")]
    End,

    /// <summary>Moves to the preceding period.</summary>
    [JsonStringEnumMemberName("pageUp")]
    PageUp,

    /// <summary>Moves to the next period.</summary>
    [JsonStringEnumMemberName("pageDown")]
    PageDown,

    /// <summary>Moves the focused date back a year.</summary>
    [JsonStringEnumMemberName("shiftPageUp")]
    ShiftPageUp,

    /// <summary>Moves the focused date forward a year.</summary>
    [JsonStringEnumMemberName("shiftPageDown")]
    ShiftPageDown,

    /// <summary>Moves to the first day of the year.</summary>
    [JsonStringEnumMemberName("controlHome")]
    ControlHome,

    /// <summary>Moves to the last day of the year.</summary>
    [JsonStringEnumMemberName("controlEnd")]
    ControlEnd,

    /// <summary>Moves keyboard focus backward.</summary>
    [JsonStringEnumMemberName("shiftTab")]
    ShiftTab,

    /// <summary>Moves keyboard focus forward.</summary>
    [JsonStringEnumMemberName("tab")]
    Tab,
}
