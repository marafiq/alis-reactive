namespace Alis.Reactive.Framework.App.Realtime;

public sealed class WardLine : SignalRHub
{
    public Event<LineCall> Welcomed => Event<LineCall>();

    public Event Found => Event();

    public Event<LineNote> Noted => Event<LineNote>();

    public void Drop() => Send();

    public void HangUp() => Send();

    public void Find(Arg<string> connectionId) => Send(connectionId);

    public void Note(Arg<string> text) => Send(text);

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

public sealed class WardBoard : ServerPushStream
{
    public Event<LineCall> Welcome => Event<LineCall>();

    public Event<LineNote> News => Event<LineNote>();

    public Event Bulletin => Event();

    public Event AllClear => Event();
}

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

public sealed class LinePage(bool offersRetry)
{
    public string World { get; } = Guid.NewGuid().ToString("N");

    public bool OffersRetry { get; } = offersRetry;
}

public sealed class WardPage(bool pageListens)
{
    public string World { get; } = Guid.NewGuid().ToString("N");

    public bool PageListens { get; } = pageListens;
}

public sealed class BedCard(string world)
{
    public string World { get; } = world;
}

public sealed class CallPointCard(string world)
{
    public string World { get; } = world;
}

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
