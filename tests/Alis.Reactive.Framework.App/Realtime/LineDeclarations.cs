namespace Alis.Reactive.Framework.App.Realtime;

/// <summary>The line to the ward: a hub that welcomes each call it takes, and that the page can ask to drop it or hang up.</summary>
public sealed class WardLine : SignalRHub
{
    public Event<LineCall> Welcomed => Event<LineCall>();

    public Event Found => Event();

    public Event<LineNote> Noted => Event<LineNote>();

    /// <summary>The server drops the call; the page calls again.</summary>
    public void Drop() => Send();

    /// <summary>The server hangs up and takes no call from the page until it answers again.</summary>
    public void HangUp() => Send();

    /// <summary>The server calls back the page it knows by this connection id.</summary>
    public void Find(Arg<string> connectionId) => Send(connectionId);

    public void Note(Arg<string> text) => Send(text);

    /// <summary>The server answers a call from the bed.</summary>
    public TypedSource<string> CallTheNurse() => Invoke<string>();
}

public sealed class LineCall : JsObject
{
    public ReadOnly<int> Call => ReadOnly<int>();
}

public sealed class LineNote : JsObject
{
    public ReadOnly<string> Text => ReadOnly<string>();
}

/// <summary>The ward's board: a stream that welcomes each call it takes, and the bulletins and news the server pushes on it.</summary>
public sealed class WardBoard : ServerPushStream
{
    public Event<LineCall> Welcome => Event<LineCall>();

    public Event<LineNote> News => Event<LineNote>();

    public Event Bulletin => Event();

    public Event AllClear => Event();
}

/// <summary>How many of a page's line and board connections the server holds open, and how many calls to the line it refused, as they change.</summary>
public sealed class Switchboard : ServerPushStream
{
    public Event<LineCounts> Lines => Event<LineCounts>();
}

public sealed class LineCounts : JsObject
{
    public ReadOnly<int> Hub => ReadOnly<int>();

    public ReadOnly<int> Board => ReadOnly<int>();

    public ReadOnly<int> Refused => ReadOnly<int>();
}

/// <summary>
/// The line page: a line and a board the page listens to, the offer to retry them when they drop, and how many
/// calls to the line the server refused.
/// </summary>
public sealed class LinePage(bool offersRetry)
{
    public string World { get; } = Guid.NewGuid().ToString("N");

    public bool OffersRetry { get; } = offersRetry;
}

/// <summary>
/// The ward page: a bed loaded into a slot listens to the line and the board, a call point loaded into another
/// calls the line and listens to none of it, and the page may listen to the line and call it too.
/// </summary>
public sealed class WardPage(bool pageListens)
{
    public string World { get; } = Guid.NewGuid().ToString("N");

    public bool PageListens { get; } = pageListens;
}

/// <summary>A bed's monitor, loaded into the ward's slot: it listens to the ward's line and board.</summary>
public sealed class BedCard(string world)
{
    public string World { get; } = world;
}

/// <summary>A bed's call point, loaded into the ward's slot: it calls the ward's line and listens to none of it.</summary>
public sealed class CallPointCard(string world)
{
    public string World { get; } = world;
}

/// <summary>The line's document events, named by their members or by the name given.</summary>
public static class LineEvents
{
    public static readonly AppEvent HungUp = AppEvent.Declare();

    public static readonly AppEvent<LineCallMade> Welcomed = AppEvent.Declare<LineCallMade>();

    public static readonly AppEvent Answered = AppEvent.Declare(name: "line:answered");
}

public sealed class LineCallMade
{
    public int Call { get; set; }
}
