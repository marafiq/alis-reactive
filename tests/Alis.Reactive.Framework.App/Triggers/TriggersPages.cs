namespace Alis.Reactive.Framework.App.Triggers;

/// <summary>A resident arriving, the page opened with what the arrival is about in its query string.</summary>
public sealed class ArrivalPage;

/// <summary>The query the arrival page is opened with, as its action would bind it with [FromQuery].</summary>
public sealed class ArrivalQuery
{
    public string? Resident { get; set; }

    public string? Wing { get; set; }

    public IReadOnlyList<string> Wings { get; set; } = new List<string>();

    public int Floor { get; set; }

    public string? Visitor { get; set; }
}

/// <summary>A card loaded into the arrival page, with a ready handler of its own.</summary>
public sealed class VisitCard;

/// <summary>A page that announces the app's events and listens for them.</summary>
public sealed class AnnouncementsPage;

public sealed class Admitted
{
    public string Name { get; set; } = "";

    public int Room { get; set; }
}

public sealed class Moved
{
    public RoomRef From { get; set; } = new();

    public List<Mover> Lines { get; set; } = new();

    /// <summary>Who escorts the move, an array by position.</summary>
    public string[] Escorts { get; set; } = [];
}

public sealed class RoomRef
{
    public int Room { get; set; }
}

public sealed class Mover
{
    public string Name { get; set; } = "";
}

public static class RoundEvents
{
    public static readonly AppEvent Started = AppEvent.Declare();

    public static readonly AppEvent<Admitted> Admitted = AppEvent.Declare<Admitted>();

    public static readonly AppEvent<Moved> Moved = AppEvent.Declare<Moved>();
}
