namespace Alis.Reactive.Framework.App.Subscriptions;

/// <summary>The front gate's board: announcements, raised by the board's own markup when it is clicked.</summary>
public sealed class GatePage;

/// <summary>An announcement at the gate, of a type when the board says which.</summary>
public sealed class Announcement
{
    public string? Type { get; set; }
}

public static class GateEvents
{
    public static readonly AppEvent<Announcement> Announced = AppEvent.Declare<Announcement>();
}
