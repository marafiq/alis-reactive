namespace Alis.Reactive.Framework.App.Requests;

public sealed class RouteRefusalsPage
{
    public RouteMistake Mistake { get; set; }

    public int RoomId { get; set; } = 214;

    public List<int> Rooms { get; set; } = new() { 214, 215 };

    public RoomGuide Guide { get; set; } = new();
}

public enum RouteMistake
{
    LeaveAPlaceholderUnfilled,
    CloseABraceNeverOpened,
    OpenABraceNeverClosed,
    NameAPlaceholderBadly,
    FillAPlaceholderWithAList,
    SendARecordAsAHeader,
    ReadARecordFromTheQuery,
    ReadTheWholeQuery,
}

public sealed class RoomGuide
{
    public string? Name { get; set; }
}

public sealed class RoomLookup
{
    public int RoomId { get; set; }

    public List<int>? Rooms { get; set; }
}

public sealed class RoomQuery
{
    public string? Wing { get; set; }

    public RoomGuide? Guide { get; set; }
}
