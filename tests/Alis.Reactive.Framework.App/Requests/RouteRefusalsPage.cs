namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A room lookup whose request makes one mistake a route, a header or a query cannot carry, so it is refused when it renders.</summary>
public sealed class RouteRefusalsPage
{
    public RouteMistake Mistake { get; set; }

    public int RoomId { get; set; } = 214;

    public List<int> Rooms { get; set; } = new() { 214, 215 };

    public RoomGuide Guide { get; set; } = new();
}

/// <summary>The mistakes a route, a header or a query can make.</summary>
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

/// <summary>The guide a room lookup names: a record, not one value.</summary>
public sealed class RoomGuide
{
    public string? Name { get; set; }
}

/// <summary>The lookup as the endpoint binds it: the room by id, or the rooms.</summary>
public sealed class RoomLookup
{
    public int RoomId { get; set; }

    public List<int>? Rooms { get; set; }
}

/// <summary>The page's query string, as the page reads it.</summary>
public sealed class RoomQuery
{
    public string? Wing { get; set; }

    public RoomGuide? Guide { get; set; }
}
