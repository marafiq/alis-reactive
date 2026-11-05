namespace Alis.Reactive.Framework.App.Triggers;

public sealed class ArrivalPage;

public sealed class ArrivalQuery
{
    public string? Resident { get; set; }

    public string? Wing { get; set; }

    public IReadOnlyList<string> Wings { get; set; } = new List<string>();

    public int Floor { get; set; }

    public string? Visitor { get; set; }
}

public sealed class VisitCard;

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
