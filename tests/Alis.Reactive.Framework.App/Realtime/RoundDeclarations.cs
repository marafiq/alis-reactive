namespace Alis.Reactive.Framework.App.Realtime;

/// <summary>The round's news, pushed by the server as server-sent events.</summary>
public sealed class RoundStream : ServerPushStream
{
    public Event<RoundNews> News => Event<RoundNews>();

    public Event AllClear => Event();
}

public sealed class RoundNews : JsObject
{
    public ReadOnly<string> Message => ReadOnly<string>();

    public ReadOnly<int> Count => ReadOnly<int>();
}

/// <summary>The round's hub: the methods the server calls on the page, and those the page calls on the server.</summary>
public sealed class RoundFeed : SignalRHub
{
    public Event<RoundStatus> StatusChanged => Event<RoundStatus>();

    public Event BoardCleared => Event();

    public Event Found => Event();

    public void ClearBoard() => Send();

    /// <summary>The server's ClearBoard, under another member's name.</summary>
    public void Sweep() => Send(name: "ClearBoard");

    public void Ping(Arg<string> resident) => Send(resident);

    public TypedSource<string> Acknowledge(Arg<string> nurse) => Invoke<string>(nurse);

    public void Tidy() => Invoke();

    public void TidyFor(Arg<string> resident) => Invoke(resident);

    public TypedSource<string> Roster() => Invoke<string>();

    /// <summary>The server calls back the page it knows by this connection id, which a page reads only while connected.</summary>
    public void Find(Arg<string?> connectionId) => Send(connectionId);

    /// <summary>The server calls StatusChanged with a bare word instead of one object, then BoardCleared.</summary>
    public void AskForABareStatus() => Send();
}

public sealed class RoundStatus : JsObject
{
    public ReadOnly<string> Resident => ReadOnly<string>();

    public ReadOnly<string> Status => ReadOnly<string>();
}

/// <summary>The round page: its world keeps its stream apart from every other page's.</summary>
public sealed class RoundPage
{
    public string World { get; } = Guid.NewGuid().ToString("N");
}
