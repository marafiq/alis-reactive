using Alis.Reactive.Conformance.World;
using Alis.Reactive.Declarations;
using Alis.Reactive.Realtime;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed record LiveLine(string Ward, string Layout, bool PageNamesTheBedLine);

public sealed record StatusCard(string Resident, string Key);

public sealed record LineCard(string Bed);

public sealed class WardLine : SignalRHub
{
    public static readonly SignalRSource<WardLine> Live = At<WardLine>(CareEndpoints.WardLine);

    public HubPush<Welcome> Welcome => Push<Welcome>();

    public HubPush<StatusChange> StatusChanged => Push<StatusChange>();

    public MethodResult<string> Roster() => Invoke<string>();
}

public sealed class RoundFeed : SignalRHub
{
    public static readonly SignalRSource<RoundFeed> Live = At<RoundFeed>(CareEndpoints.RoundFeed);

    public HubPush<Welcome> Welcome => Push<Welcome>();

    public HubPush<StatusChange> StatusChanged => Push<StatusChange>();
}

public sealed class BedLine : SignalRHub
{
    public static readonly SignalRSource<BedLine> Live = At<BedLine>(CareEndpoints.BedLine);

    public HubPush<Welcome> Welcome => Push<Welcome>();
}

public sealed class CardLine : SignalRHub
{
    public static readonly SignalRSource<CardLine> Live = At<CardLine>(CareEndpoints.Line);

    public HubPush<Welcome> Welcome => Push<Welcome>();
}

public sealed class NurseLine : SignalRHub
{
    public static readonly SignalRSource<NurseLine> Live = At<NurseLine>(CareEndpoints.NurseLine);

    public HubPush<Welcome> Welcome => Push<Welcome>();

    public MethodResult<string> Call() => Invoke<string>();
}

public sealed class StatusChange
{
    public string Resident { get; set; } = "";

    public string Status { get; set; } = "";
}

public static class LiveLineButtons
{
    public static readonly AppEvent AskForTheRoster = AppEvent.Declare();

    public static readonly AppEvent OpenAdasCard = AppEvent.Declare();

    public static readonly AppEvent OpenBensCard = AppEvent.Declare();

    public static readonly AppEvent CloseAdasCard = AppEvent.Declare();

    public static readonly AppEvent OpenTheCard = AppEvent.Declare();

    public static readonly AppEvent CallNurse = AppEvent.Declare();
}
