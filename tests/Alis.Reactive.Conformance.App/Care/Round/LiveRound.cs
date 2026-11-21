using Alis.Reactive.Conformance.World;
using Alis.Reactive.Realtime;
using Alis.Reactive.TriggersAndPipelines;

namespace Alis.Reactive.Conformance.App.Care;

public sealed record LiveRound(string Ward, string Layout);

public sealed record BedCard(string Bed);

public sealed record GivenDose(string Name);

public sealed record SavedItem(string Name);

public sealed class WardBoardStream : ServerPushStream
{
    public static readonly ServerPushSource<WardBoardStream> Live = At<WardBoardStream>(CareEndpoints.WardBoard);

    public ServerEvent<Welcome> Welcome => Message<Welcome>();

    public ServerEvent<Bulletin> Bulletin => Message<Bulletin>();

    public ServerEvent<AllClear> AllClear => Message<AllClear>();

    public ServerEvent<News> News => Message<News>();
}

public sealed class RoundNews : ServerPushStream
{
    public static readonly ServerPushSource<RoundNews> Live = At<RoundNews>(CareEndpoints.RoundNews);

    public ServerEvent<Welcome> Welcome => Message<Welcome>();

    public ServerEvent<Dose> Dose => Message<Dose>();
}

public sealed class Feed : ServerPushStream
{
    public static readonly ServerPushSource<Feed> Live = At<Feed>(CareEndpoints.Feed);

    public ServerEvent<Welcome> Welcome => Message<Welcome>();

    public ServerEvent<Item> Item => Message<Item>();
}

public sealed class FacilityAlerts : ServerPushStream
{
    public static readonly ServerPushSource<FacilityAlerts> Live = At<FacilityAlerts>(CareEndpoints.FacilityAlerts);

    public ServerEvent<Welcome> Welcome => Message<Welcome>();

    public ServerEvent<Alert> Alert => Message<Alert>();
}

public sealed class BedBoard : ServerPushStream
{
    public static readonly ServerPushSource<BedBoard> Live = At<BedBoard>(CareEndpoints.BedBoard);

    public ServerEvent<Welcome> Welcome => Message<Welcome>();
}

public sealed class Switchboard : ServerPushStream
{
    public static readonly ServerPushSource<Switchboard> Live = At<Switchboard>(CareEndpoints.Switchboard);

    public ServerEvent<Lines> Lines => Message<Lines>();
}

public sealed class Welcome
{
    public int Accepted { get; set; }
}

public sealed class Bulletin
{
    public string? Text { get; set; }
}

public sealed class AllClear;

public sealed class News
{
    public string Text { get; set; } = "";
}

public sealed class Dose
{
    public string Name { get; set; } = "";
}

public sealed class Item
{
    public string Name { get; set; } = "";
}

public sealed class Alert
{
    public string Message { get; set; } = "";
}

public sealed class Lines
{
    public int Open { get; set; }
}

public static class LiveRoundButtons
{
    public static readonly AppEvent StopLiveAlerts = AppEvent.Declare();

    public static readonly AppEvent OpenTheBedCard = AppEvent.Declare();

    public static readonly AppEvent CloseTheBedCard = AppEvent.Declare();

    public static readonly AppEvent ShowTheBoardsState = AppEvent.Declare();

    public static readonly AppEvent Load = AppEvent.Declare();
}
