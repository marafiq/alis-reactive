namespace Alis.Reactive.Framework.App.Realtime;

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

public sealed class RoundFeed : SignalRHub
{
    public Event<RoundStatus> StatusChanged => Event<RoundStatus>();

    public Event BoardCleared => Event();

    public Event Found => Event();

    public void ClearBoard() => Send();

    public void Sweep() => Send(name: "ClearBoard");

    public void Ping(Arg<string> resident) => Send(resident);

    public TypedSource<string> Acknowledge(Arg<string> nurse) => Invoke<string>(nurse);

    public void Tidy() => Invoke();

    public void TidyFor(Arg<string> resident) => Invoke(resident);

    public TypedSource<string> Roster() => Invoke<string>();

    public void Find(Arg<string?> connectionId) => Send(connectionId);

    public void AskForABareStatus() => Send();
}

public sealed class RoundStatus : JsObject
{
    public ReadOnly<string> Resident => ReadOnly<string>();

    public ReadOnly<string> Status => ReadOnly<string>();
}

public sealed class RoundPage
{
    public string World { get; } = Guid.NewGuid().ToString("N");
}
