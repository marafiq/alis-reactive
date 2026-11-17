namespace Alis.Reactive.Conformance.World;

public static partial class CareEndpoints
{
    private const string Live = CarePages.Area + "round/live/endpoints/";

    public const string WardBoard = Live + "ward-board";
    public const string RoundNews = Live + "round-news";
    public const string Feed = Live + "feed";
    public const string FacilityAlerts = Live + "facility-alerts";
    public const string BedBoard = Live + "bed-board";
    public const string Switchboard = Live + "switchboard";
    public const string LiveDoses = Live + "doses";
    public const string LiveItems = Live + "items";
    public const string LiveList = Live + "list";
    public const string BedCard = Live + "bed-card";
    public const string BoardCard = Live + "board-card";
}
