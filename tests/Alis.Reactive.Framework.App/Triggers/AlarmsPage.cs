namespace Alis.Reactive.Framework.App.Triggers;

/// <summary>The call alarms board: alarms raised with an urgency, and the response each one gets.</summary>
public sealed class AlarmsPage;

/// <summary>An alarm raised from a room, with how urgent it is.</summary>
public sealed class Alarm
{
    public string Room { get; set; } = "";

    public AlarmUrgency Urgency { get; set; }
}

public enum AlarmUrgency
{
    Routine,
    Urgent,
}

public static class AlarmEvents
{
    public static readonly AppEvent<Alarm> Raised = AppEvent.Declare<Alarm>();
}
