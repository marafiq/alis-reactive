namespace Alis.Reactive.Framework.App.Subscriptions;

/// <summary>The front gate's board: announcements, raised by the board's own markup when it is clicked.</summary>
public sealed class GatePage;

/// <summary>An announcement at the gate: of a type when the board says which, the size of the party and how urgent it is.</summary>
public sealed class Announcement
{
    public string? Type { get; set; }

    public int PartySize { get; set; }

    public GateUrgency Urgency { get; set; }
}

public enum GateUrgency
{
    Routine,
    Urgent,
}

/// <summary>A visit the gate escorts in, with the visitor it escorts: a payload that holds a record.</summary>
public sealed class EscortedVisit
{
    public GateVisitor Visitor { get; set; } = new();
}

public sealed class GateVisitor
{
    public string Name { get; set; } = "";
}

public static class GateEvents
{
    public static readonly AppEvent Opened = AppEvent.Declare();

    public static readonly AppEvent<Announcement> Announced = AppEvent.Declare<Announcement>();

    public static readonly AppEvent<EscortedVisit> Escorted = AppEvent.Declare<EscortedVisit>();
}
