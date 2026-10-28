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

    public void ClearBoard() => Send();

    public void Ping(Arg<string> resident) => Send(resident);

    public TypedSource<string> Acknowledge(Arg<string> nurse) => Invoke<string>(nurse);
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
